﻿//#define UMDK_SUPPORT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using DGenInterface;
using DigitalRune.Windows.TextEditor.Document;
using DigitalRune.Windows.TextEditor.Bookmarks;
using DigitalRune.Windows.TextEditor.Highlighting;
using DigitalRune.Windows.TextEditor.Markers;
using System.Text.RegularExpressions;
using System.IO;
using MDStudio.Properties;

#if UMDK_SUPPORT
    using UMDK;
#endif

namespace MDStudio
{
    //  Code line regex
    //  \s*?([a-zA-Z.]+)\s+([a-zA-Z._0-9#$*+<>\(\)]+)\s*?,? ?([a-zA-Z._0-9#$\(\)]+)?
    public partial class MainForm : Form
    {
        enum State
        {
            kStopped,
            kRunning,
            kDebugging,
            kPaused
        };

        enum BreakMode
        {
            kBreakpoint,
            kStepOver,
            kLogPoint
        };

        enum SourceMode
        {
            Source,
            Disassembly
        }

        private readonly Timer m_Timer = new Timer();
        private string m_ProjectFile;
        private string m_PathToProject;
        private string m_ProjectName;
        private string m_FileToAssemble;        // File to assemble
        private string m_CurrentlyEditingFile;  // Currently editing source file
        private List<string> m_ProjectFiles;
        private string m_BuildArgs;

        public Target m_Target;
        public Symbols m_DebugSymbols;
        private string m_DisassemblyText;
        private List<Tuple<uint, int, string>> m_Disassembly;
        private static int s_MaxDisassemblyLines = 200;
        private uint m_DisassembledFrom = 0;
        private uint m_DisassembledTo = 0;

        private RegisterView m_RegisterView;
        private MemoryView m_MemoryView;
        private BuildLog m_BuildLog;
        private CRamViewer m_CRAMViewer;
        private VDPStatusWindow m_VDPStatus;
        private ProfilerView m_ProfilerView;
        private BreakpointView m_BreakpointView;

        private Config m_Config;

        private VDPRegs m_VDPRegs;

        private bool m_Modified;

        private List<Marker> m_ErrorMarkers;

        private List<Marker> m_SearchMarkers;
        private List<TextLocation> m_SearchResults;
        private string m_ReplaceString;
        private int m_SearchIndex;
        
        private State m_State = State.kStopped;

        private FileSystemWatcher m_SourceWatcher;
        private FileSystemEventHandler m_OnFileChanged;
        private Object m_WatcherCritSec = new object();

        class Breakpoint
        {
            public Breakpoint(string _file, int _line) { filename = _file; line = _line; address = 0; originalLine = 0; }
            public Breakpoint(string _file, int _line, uint _address) { filename = _file; line = _line; address = _address; originalLine = 0; }
            public string filename;
            public int line;
            public int originalLine;
            public uint address;
        }

        private BreakMode m_BreakMode = BreakMode.kBreakpoint;
        private SourceMode m_SourceMode = SourceMode.Source;
        private Breakpoint m_StepOverBreakpoint;

        private List<Breakpoint> m_Breakpoints;
        private List<uint> m_Watchpoints;

#if UMDK_SUPPORT
        private static UMDKInterface m_UMDK = null;
#endif

        public class ProfilerEntry
        {
            public uint address { get; set; }
            public uint hitCount { get; set; }
            public uint cyclesPerHit { get; set; }
            public uint totalCycles { get; set; }
            public float percentCost { get; set; }
            public string filename { get; set; }
            public int line { get; set; }
        };

        private List<ProfilerEntry> m_ProfileResults;
        private bool m_Profile;

        public static readonly ReadOnlyCollection<Tuple<int, int>> kValidResolutions = new ReadOnlyCollection<Tuple<int, int>>(new[]
        {
            new Tuple<int,int>( 320, 240 ),
            new Tuple<int,int>( 640, 480 ),
            new Tuple<int,int>( 960, 720 ),
            new Tuple<int,int>( 1280, 720 ),
        });

        public static readonly ReadOnlyCollection<Tuple<char, string>> kRegions = new ReadOnlyCollection<Tuple<char, string>>(new[]
        {
            new Tuple<char, string>( 'J', "Japan" ),
            new Tuple<char, string>( 'U', "USA" ),
            new Tuple<char, string>( 'E', "Europe" )
        });

        //Default config
        public const int kDefaultResolutionEntry = 1;
        public const int kDefaultRegion = 0;

        //Memory preview in register window
        public const int kMaxMemPreviewSize = 16;

        unsafe struct MemPreviewBuffer
        {
            public fixed byte dataBuffer[kMaxMemPreviewSize];
        }

        private static readonly ReadOnlyCollection<string> kStepIntoInstrs = new ReadOnlyCollection<string>(new[]
        {
            "RTS",
            "JMP",
            "BRA",
            "BCC",
            "BCS",
            "BEQ",
            "BGE",
            "BGT",
            "BHI",
            "BHS",
            "BLE",
            "BLS",
            "BLT",
            "BLO",
            "BMI",
            "BNE",
            "BPL",
            "BVC",
            "BVS",
            "DBCC",
            "DBCS",
            "DBEQ",
            "DBGE",
            "DBGT",
            "DBHI",
            "DBLE",
            "DBLS",
            "DBLT",
            "DBMI",
            "DBNE",
            "DBPL",
            "DBVC",
            "DBVS",
            "DBRA",
        });

        public MainForm()
        {
            InitializeComponent();
            
            m_ErrorMarkers = new List<Marker>();
            m_SearchMarkers= new List<Marker>();
            m_SearchResults = new List<TextLocation>();
            m_Breakpoints = new List<Breakpoint>();
            m_Watchpoints = new List<uint>();

            //
            m_Config = new Config();
            m_Config.Read();

            m_VDPRegs = new VDPRegs(this);

            //
            m_BuildLog = new BuildLog(this);
            m_BuildLog.Hide();

            //Show profiler results
            m_ProfilerView = new ProfilerView(this);
            m_ProfilerView.Hide();

            //
            m_RegisterView = new RegisterView();
            m_RegisterView.Hide();

            m_CRAMViewer = new CRamViewer(this);
            if(Settings.Default.CRAMWindowVisible)
                m_CRAMViewer.Show();
            else
                m_CRAMViewer.Hide();

            m_VDPStatus = new VDPStatusWindow(this);
            if (Settings.Default.VDPStatusWindowVisible)
                m_VDPStatus.Show();
            else
                m_VDPStatus.Hide();

            // Set the syntax-highlighting for C#
            codeEditor.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("ASM68k");

            //Create target
            try
            {
                m_Target = TargetFactory.Create(m_Config.TargetName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not create target of type \'" + m_Config.TargetName + "\', defaulting to TargetDGen");
                m_Target = new TargetDGen();
                m_Config.TargetName = typeof(TargetDGen).Name;
            }

            m_Timer.Interval = 16;
            m_Timer.Tick += TimerTick;
            m_Timer.Enabled = true;

            // Setup file changed watcher
            m_OnFileChanged = new FileSystemEventHandler(OnSourceChanged);

            if (m_Config.AutoOpenLastProject)
            {
                //Open last project
                OpenProject(m_Config.LastProject);
            }

            StopDebugging();

#if UMDK_SUPPORT
            m_UMDK = new UMDKInterface();
#endif
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void OpenFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                // Ignore document changed events
                bool watchingEvents = (m_SourceWatcher == null) || m_SourceWatcher.EnableRaisingEvents;
                m_SourceWatcher = null;
                codeEditor.Document.DocumentChanged -= documentChanged;

                // Reset undo state
                m_Modified = false;
                undoMenu.Enabled = false;

                // Load file
                m_CurrentlyEditingFile = filename;
                codeEditor.LoadFile(filename);

                // Set title bar text
                this.Text = "MDStudio - " + m_CurrentlyEditingFile;

                // Populate known breakpoint markers
                foreach (Breakpoint breakpoint in m_Breakpoints)
                {
                    if (breakpoint.line != 0 && breakpoint.filename.ToLower() == m_CurrentlyEditingFile.ToLower())
                    {
                        if (!codeEditor.Document.BookmarkManager.IsMarked(breakpoint.line))
                            codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, breakpoint.line));
                    }
                }

                // Resubscribe to document changed events
                codeEditor.Document.DocumentChanged += documentChanged;
                m_SourceWatcher = new FileSystemWatcher();
                m_SourceWatcher.Path = Path.GetDirectoryName(m_CurrentlyEditingFile);
                m_SourceWatcher.Filter = Path.GetFileName(m_CurrentlyEditingFile);
                m_SourceWatcher.EnableRaisingEvents = watchingEvents;
                m_SourceWatcher.NotifyFilter = NotifyFilters.LastWrite;
                m_SourceWatcher.Changed += m_OnFileChanged;

                // Refresh
                codeEditor.Refresh();

                m_SourceMode = SourceMode.Source;
            }
        }

        private void SetDisassemblyText(string text)
        {
            // Ignore document changed events
            bool watchingEvents = (m_SourceWatcher == null) || m_SourceWatcher.EnableRaisingEvents;
            m_SourceWatcher = null;
            codeEditor.Document.DocumentChanged -= documentChanged;

            // Reset undo state
            m_Modified = false;
            undoMenu.Enabled = false;

            // Load file
            m_CurrentlyEditingFile = "Disassembly";
            codeEditor.Text = text;

            // Set title bar text
            this.Text = "MDStudio - " + "Disassembly";

            // Refresh
            codeEditor.Refresh();
        }

        private void StartDebugging()
        {
            stepIntoMenu.Enabled = true;
            stepOverMenu.Enabled = true;
            stopToolStripMenuItem.Enabled = true;
            breakMenu.Enabled = true;
        }

        private void StopDebugging()
        {
            stepIntoMenu.Enabled = false;
            stepOverMenu.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            breakMenu.Enabled = false;
            m_Watchpoints.Clear();
        }

        private void SetTargetBreakpoint(uint address)
        {
            if (m_Target != null)
            {
                m_Target.AddBreakpoint(address);
            }
        }

        private void RemoveTargetBreakpoint(uint address)
        {
            if (m_Target != null)
            {
                m_Target.RemoveBreakpoint(address);
            }
        }

        public void SetBreakpoint(uint address)
        {
            if(m_BreakpointView != null)
            {
                m_BreakpointView.SetBreakpoint(address);
            }

            SetTargetBreakpoint(address);

            Tuple<string, int, int> fileLine = m_DebugSymbols.GetFileLine(address);

            m_Breakpoints.Add(new Breakpoint(fileLine.Item1, fileLine.Item3));
        }

        public int SetBreakpoint(string filename, int line)
        {
            if (m_State != State.kStopped && m_DebugSymbols != null)
            {
                //Symbols loaded, add "online" breakpoint by address
                uint address = m_DebugSymbols.GetAddress(filename, line);
                if (address >= 0)
                {
                    if (m_BreakpointView != null)
                    {
                        //TODO: Breakpoint view should be by file/line, not address
                        m_BreakpointView.SetBreakpoint(address);
                    }

                    SetTargetBreakpoint(address);
                    m_Breakpoints.Add(new Breakpoint(filename, line, address));

                    //Actual line might be different from requested line, look it up
                    return m_DebugSymbols.GetFileLine(address).Item3;
                }

                return line;
            }
            else
            {
                //No symbols yet, add "offline" breakpoint by file/line
                m_Breakpoints.Add(new Breakpoint(filename, line));
                return line;
            }
        }

        public void RemoveBreakpoint(uint address)
        {
            if (m_BreakpointView != null)
            {
                //TODO: Breakpoint view should be by file/line, not address
                m_BreakpointView.RemoveBreakpoint(address);
            }

            RemoveTargetBreakpoint(address);

            Tuple<string, int, int> fileLine = m_DebugSymbols.GetFileLine(address);

            m_Breakpoints.Remove(m_Breakpoints.Find(breakpoint => breakpoint.filename.Equals(fileLine.Item1, StringComparison.OrdinalIgnoreCase) && breakpoint.line >= fileLine.Item2 && breakpoint.line <= fileLine.Item3));
        }

        public int RemoveBreakpoint(string filename, int line)
        {
            if (m_State != State.kStopped && m_DebugSymbols != null)
            {
                uint address = m_DebugSymbols.GetAddress(filename, line);
                if (address >= 0)
                {
                    if (m_BreakpointView != null)
                    {
                        //TODO: Breakpoint view should be by file/line, not address
                        m_BreakpointView.RemoveBreakpoint(address);
                    }

                    RemoveTargetBreakpoint(address);

                    m_Breakpoints.Remove(m_Breakpoints.Find(breakpoint => breakpoint.filename.Equals(filename, StringComparison.OrdinalIgnoreCase) && breakpoint.line == line));

                    //Actual line might be different from requested line, look it up
                    return m_DebugSymbols.GetFileLine(address).Item3;
                }

                return line;
            }
            else
            {
                m_Breakpoints.Remove(m_Breakpoints.Find(breakpoint => breakpoint.filename.Equals(filename, StringComparison.OrdinalIgnoreCase) && breakpoint.line == line));
                return line;
            }
        }

        public void ClearBreakpoints()
        {
            if (m_BreakpointView != null)
            {
                m_BreakpointView.ClearBreakpoints();
            }

            if (m_Target != null)
            {
                m_Target.RemoveAllBreakpoints();
            }

            m_Breakpoints.Clear();
        }

        private void UpdateCRAM()
        {
            if(m_Target is EmulatorTarget)
            {
                // Update palette
                if (m_CRAMViewer.Visible)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        uint rgb = (m_Target as EmulatorTarget).GetColor(i);
                        m_CRAMViewer.SetColor(i, rgb);
                    }
                }
            }
        }

        private void DumpZ80State()
        {
            Console.WriteLine("Z80 fa = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_FA));
            Console.WriteLine("Z80 cb = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_CB));
            Console.WriteLine("Z80 ed = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_ED));
            Console.WriteLine("Z80 lh = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_LH));
            Console.WriteLine("Z80 fa' = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_FA_ALT));
            Console.WriteLine("Z80 cb' = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_CB_ALT));
            Console.WriteLine("Z80 ed' = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_ED_ALT));
            Console.WriteLine("Z80 lh' = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_LH_ALT));
            Console.WriteLine("Z80 ix = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_IX));
            Console.WriteLine("Z80 iy = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_IY));
            Console.WriteLine("Z80 sp = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_SP));
            Console.WriteLine("Z80 pc = 0x{0:x}", m_Target.GetZ80Reg(Z80Regs.Z80_REG_PC));
            Console.WriteLine("1FFF = 0x{0:x}", m_Target.ReadZ80Byte((int)0x1FFF));
            Console.WriteLine("1FFB = 0x{0:x}", m_Target.ReadZ80Byte((int)0x1FFB));
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (
                (m_Target != null && m_Target.IsHalted())
#if UMDK_SUPPORT
//                 || ()
#endif
                && m_State == State.kRunning)
            {
                if(m_BreakMode == BreakMode.kLogPoint && m_Watchpoints.Count > 0)
                {
                    //Log point, fetch new value, log and continue
                    uint value = m_Target.ReadLong(m_Watchpoints[0]);
                    String log = String.Format("LOGPOINT - Address 0x{0:x} = 0x{1:x}", m_Watchpoints[0], value);
                    Console.WriteLine(log);
                    m_Target.Resume();
                }
                else
                {
                    //Breakpoint hit, go to address
                    uint currentPC = m_Target.GetPC();
                    GoTo(currentPC);

                    //Get regs
                    uint[] dregs = new uint[8];
                    for (int i = 0; i < 8; i++)
                    {
                        dregs[i] = m_Target.GetDReg(i);
                    }

                    uint[] aregs = new uint[8];
                    for (int i = 0; i < 8; i++)
                    {
                        aregs[i] = m_Target.GetAReg(i);
                    }

                    uint sr = m_Target.GetSR();

                    m_RegisterView.SetRegs(dregs[0], dregs[1], dregs[2], dregs[3], dregs[4], dregs[5], dregs[6], dregs[7],
                                            aregs[0], aregs[1], aregs[2], aregs[3], aregs[4], aregs[5], aregs[6], aregs[7], 0,
                                            sr, (uint)currentPC);

                    m_RegisterView.SetZ80Regs(
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_FA),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_CB),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_ED),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_LH),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_FA_ALT),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_CB_ALT),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_ED_ALT),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_LH_ALT),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_IX),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_IY),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_SP),
                        m_Target.GetZ80Reg(Z80Regs.Z80_REG_PC));

                    //Read stack
                    int maxStack = 20;
                    uint stackTop = m_Target.ReadLong(0x0000) & 0x00FFFFFF;
                    uint stackAddr = aregs[7] & 0x00FFFFFF;
                    List<Tuple<uint,uint>> stack = new List<Tuple<uint, uint>>();

                    //ReadLong is returning bad values, hack to use byte read for now
                    uint ReadLong(uint address)
                    {
                        byte[] buffer = new byte[4];
                        m_Target.ReadMemory(address, 4, buffer);
                        return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
                    }

                    for (int i = 0; i < maxStack; i++)
                    {
                        uint stackValue = ReadLong(stackTop);
                        stack.Insert(0, new Tuple<uint, uint>(stackTop, stackValue));

                        stackTop -= 2;
                        if (stackAddr == stackTop)
                        {
                            //Stack ended on word boundary, read last word
                            stackValue = (uint)m_Target.ReadWord(stackTop) << 16;
                            stack.Insert(0, new Tuple<uint, uint>(stackTop, stackValue));
                            break;
                        }

                        stackTop -= 2;
                        if (stackAddr == stackTop)
                        {
                            //Stack ended on word boundary, read last long
                            stackValue = ReadLong(stackTop);
                            stack.Insert(0, new Tuple<uint, uint>(stackTop, stackValue));
                            break;
                        }
                    }

                    m_RegisterView.SetStack(stack);

                    //Dereference ARegs and fill memory previews
                    unsafe
                    {
                        byte[] localBuffer = new byte[kMaxMemPreviewSize];

                        m_Target.ReadMemory(aregs[0], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a0(localBuffer);

                        m_Target.ReadMemory(aregs[1], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a1(localBuffer);

                        m_Target.ReadMemory(aregs[2], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a2(localBuffer);

                        m_Target.ReadMemory(aregs[3], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a3(localBuffer);

                        m_Target.ReadMemory(aregs[4], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a4(localBuffer);

                        m_Target.ReadMemory(aregs[5], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a5(localBuffer);

                        m_Target.ReadMemory(aregs[6], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_a6(localBuffer);

                        m_Target.ReadMemory(aregs[7], kMaxMemPreviewSize, localBuffer);
                        m_RegisterView.SetData_sp(localBuffer);
                    }

                    //Set status
                    statusLabel.Text = "PC 0x" + m_Target.GetPC();

                    //Bring window to front
                    BringToFront();

                    UpdateCRAM();
                    m_VDPStatus.UpdateView();

                    //Determine break mode
                    if (m_BreakMode == BreakMode.kStepOver)
                    {
                        //If hit desired step over address
                        if (currentPC == m_StepOverBreakpoint.address)
                        {
                            //Clear step over breakpoint, if we don't have a user breakpoint here
                            if (m_Breakpoints.IndexOf(m_StepOverBreakpoint) == -1)
                            {
                                m_Target.RemoveBreakpoint(m_StepOverBreakpoint.address);
                            }

                            //Return to breakpoint mode
                            m_StepOverBreakpoint = null;
                            m_BreakMode = BreakMode.kBreakpoint;
                        }
                        else
                        {
                            Console.WriteLine("Step-over hit unexpected breakpoint at " + currentPC);
                        }
                    }

                    //In breakpoint state
                    m_State = State.kDebugging;
                }
            }
            else if (m_Target != null && m_State == State.kRunning)
            {
                UpdateCRAM();
            }
        }

        List<string> ScanIncludes(string rootPath, string filename)
        {
            List<string> includes = new List<string>();
            List<string> localIncludes = new List<string>();

            try
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(filename))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        // all whitespace, followed by 'include', followed by all whitespace, followed by filename in quotes (relative to first assembled file)
                        // e.g. "	include '..\framewk\dmaqueue.asm'"
                        string pattern = "^\\s*include(\\s+)*[\'\\\"](.+)*[\'\\\"]";
                        Match match = Regex.Match(line, pattern);

                        if (match.Success)
                        {
                            //Convert relative paths to absolute
                            string include = System.IO.Path.GetFullPath(System.IO.Path.Combine(rootPath, match.Groups[2].Value));

                            //If absolute path doesn't exist, try each include directory
                            if (!System.IO.File.Exists(include))
                            {
                                if (m_Config.Asm68kIncludePaths != null)
                                {
                                    foreach (string includePath in m_Config.Asm68kIncludePaths)
                                    {
                                        string fullPath = System.IO.Path.Combine(includePath, match.Groups[2].Value);

                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            include = fullPath;
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            includes.Add(include);
                            localIncludes.Add(include);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("build exception: " + e.Message);
            }

            //Recurse
            foreach (string include in localIncludes)
            {
                includes.AddRange(ScanIncludes(rootPath, include));
            }

            return includes;
        }

        private void PopulateFileView()
        {
            //Scan for includes
            m_ProjectFiles = ScanIncludes(m_PathToProject, m_ProjectFile);

            //Add current file and sort
            m_ProjectFiles.Add(m_CurrentlyEditingFile);
            m_ProjectFiles.Sort();

            //Populate view
            TreeNode lastNode = null;
            string subPathAgg;

            foreach (string path in m_ProjectFiles)
            {
                if (path != null)
                {
                    subPathAgg = string.Empty;

                    foreach (string subPath in path.Split(treeProjectFiles.PathSeparator[0]))
                    {
                        subPathAgg += subPathAgg.Length > 0 ? treeProjectFiles.PathSeparator[0] + subPath : subPath;
                        string absPath = System.IO.Path.GetFullPath(subPathAgg);

                        TreeNode[] nodes = treeProjectFiles.Nodes.Find(absPath, true);
                        if (nodes.Length == 0)
                        {
                            if (lastNode == null)
                                lastNode = treeProjectFiles.Nodes.Add(absPath, subPath);
                            else
                                lastNode = lastNode.Nodes.Add(absPath, subPath);
                        }
                        else
                        {
                            lastNode = nodes[0];
                        }
                    }
                }

                lastNode = null;
            }
        }

        private void undoMenu_Click(object sender, EventArgs e)
        {
            codeEditor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            codeEditor.Redo();
        }

        private void codeEditor_DocumentChanged(object sender, DocumentEventArgs e)
        {
            if (codeEditor.Document.UndoStack.CanUndo)
            {
                undoMenu.Enabled = true;
            }
            else
            {
                undoMenu.Enabled = false;
            }

            if (codeEditor.Document.UndoStack.CanRedo)
            {
                redoMenu.Enabled = true;
            }
            else
            {
                redoMenu.Enabled = false;
            }

        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //HANDLE STDERR
            if (e.Data != null && !e.Data.Equals(""))
            {
                if (!e.Data.Contains("Something"))
                {
                }
            }
        }

        private int Build()
        {
            if (m_Config.Asm68kPath == null || m_Config.Asm68kPath.Length == 0)
            {
                MessageBox.Show("ASM68k Path not set");
                return 0;
            }

            if (!File.Exists(m_Config.Asm68kPath))
            {
                MessageBox.Show("Cannot find '" + m_Config.Asm68kPath + "'");
                return 0;
            }
            
            m_BuildLog.Clear();
            if(!m_BuildLog.Visible)
                m_BuildLog.Show();

            Console.WriteLine("compile");

            StringBuilder processStandardOutput = new StringBuilder();
            StringBuilder processErrorOutput = new StringBuilder();

            try
            {
                int timeout = 60 * 1000 * 1000;

                using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
                using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
                {
                    using (Process process = new Process())
                    {
                        string includeArgs = "";
                        if(m_Config.Asm68kIncludePaths != null && m_Config.Asm68kIncludePaths.Length > 0)
                        {
                            includeArgs += "/j";

                            foreach(string include in m_Config.Asm68kIncludePaths)
                            {
                                includeArgs += " ";
                                includeArgs += include;
                            }

                            includeArgs += "*";
                        }

                        process.StartInfo.FileName = m_Config.Asm68kPath;
                        process.StartInfo.WorkingDirectory = m_PathToProject + @"\";
                        process.StartInfo.Arguments = @"/p /c /zd " + m_Config.Asm68kArgs + " " + includeArgs + " " + m_FileToAssemble + "," + m_ProjectName + ".bin," + m_ProjectName + ".symb," + m_ProjectName + ".list";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;

                        Console.WriteLine("Assembler: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                        m_BuildLog.AddRaw(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                        m_BuildLog.Refresh();

                        process.Start();

                        try
                        {
                            process.OutputDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    outputWaitHandle.Set();
                                }
                                else
                                {
                                    processStandardOutput.AppendLine(e.Data);
                                }
                            };
                            process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    errorWaitHandle.Set();
                                }
                                else
                                {
                                    processErrorOutput.AppendLine(e.Data);
                                }
                            };

                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (!process.WaitForExit(timeout))
                            {
                                processErrorOutput.Append("Process timed out");
                            }
                        }
                        finally
                        {
                            outputWaitHandle.WaitOne(timeout);
                            errorWaitHandle.WaitOne(timeout);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("build exception: " + e.Message);
            }

            //()\((\d+)\)\s: Error : (.+)
            string[] output;
            
            if(processErrorOutput.Length > 0 )
                output = processErrorOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            else
                output = processStandardOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            int errorCount = 0;

            foreach (Marker marker in m_ErrorMarkers)
            {
                codeEditor.Document.MarkerStrategy.RemoveMarker(marker);
            }

            m_ErrorMarkers.Clear();

            foreach (string line in output)
            {
                Console.WriteLine(line);
                m_BuildLog.AddRaw(line);
                string patternError = @"([\w:\\.]*)\((\d+)\) : Error : (.+)";
                Match matchError = Regex.Match(line, patternError);

                if (matchError.Success)
                {
                    int lineNumber;

                    int.TryParse(matchError.Groups[2].Value, out lineNumber);

                    string filename = matchError.Groups[1].Value;

                    m_BuildLog.AddError(filename, lineNumber, matchError.Groups[3].Value);
                    Console.WriteLine("Error in '" + matchError.Groups[1].Value + "' (" + matchError.Groups[2].Value + "): " + matchError.Groups[3].Value);
                    errorCount++;

                    //  Mark the line
                    if(matchError.Groups[1].Value == m_FileToAssemble)
                    {
                        int offset = codeEditor.Document.PositionToOffset(new TextLocation(0, lineNumber - 1));
                        Marker marker = new Marker(offset, codeEditor.Document.LineSegmentCollection[lineNumber - 1].Length, MarkerType.SolidBlock, Color.DarkRed, Color.Black);
                        codeEditor.Document.MarkerStrategy.AddMarker(marker);

                        m_ErrorMarkers.Add(marker);
                    }
                }
            }
            Console.WriteLine(errorCount + " Error(s)");

            codeEditor.Refresh();

            string binaryFile = m_PathToProject + @"\" + m_ProjectName + ".bin";
            string symbolFile = m_PathToProject + @"\" + m_ProjectName + ".symb";

            if (errorCount > 0)
            {
                statusLabel.Text = errorCount + " Error(s)";
            }
            else if(!File.Exists(binaryFile) || !File.Exists(symbolFile))
            {
                statusLabel.Text = "Build error, no output files generated";
                errorCount++;
            }
            else
            {
                statusLabel.Text = "Build ok!";

                //Success, read symbols
                m_DebugSymbols = new Symbols();
                m_DebugSymbols.Read(symbolFile);
            }

            return errorCount;
        }

        private void compileMenu_Click(object sender, EventArgs e)
        {
            Save();
            Build();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            if (m_State == State.kDebugging)
            {
                codeEditor.Document.MarkerStrategy.Clear();
                codeEditor.ActiveTextAreaControl.Invalidate();
                codeEditor.Refresh();

                m_Target.Resume();

                statusLabel.Text = "Running...";
                //TODO: Bring emu window to front

                m_State = State.kRunning;
            }
            else if(m_State == State.kStopped)
            {
                Save();

                if (Build() == 0)
                {
                    string initialSourceFile = m_PathToProject + @"\" + m_FileToAssemble;
                    string listingFile = m_PathToProject + @"\" + m_ProjectName + ".list";
                    string symbolFile = m_PathToProject + @"\" + m_ProjectName + ".symb";
                    string baseDirectory = m_PathToProject + @"\";
                    string binaryFile = m_PathToProject + @"\" + m_ProjectName + ".bin";

                    //  Show tools windows first, so emu window gets foreground focus
                    m_RegisterView.Show();

                    //Read symbols
                    m_DebugSymbols = new Symbols();
                    m_DebugSymbols.Read(symbolFile);

                    if(m_BreakpointView != null)
                    {
                        m_BreakpointView.UpdateSymbols(m_DebugSymbols);
                    }

                    //Clear last disassembly
                    m_DisassemblyText = null;
                    m_Disassembly = null;
                    m_DisassembledFrom = 0;
                    m_DisassembledTo = 0;

#if UMDK_SUPPORT
                    if (UMDKEnabledMenuOption.Checked)
                    {

                    }
                    else
#endif  //  UMDK_SUPPORT
                    {
                        //Init emu
                        Tuple<int, int> resolution = kValidResolutions[m_Config.EmuResolution];

                        if(m_Target is EmulatorTarget)
                        {
                            (m_Target as EmulatorTarget).Initialise(resolution.Item1, resolution.Item2, this.Handle, m_Config.Pal, kRegions[m_Config.EmuRegion].Item1);
                        }

                        //Set input mappings
                        if(m_Target is EmulatorTarget)
                        {
                            EmulatorTarget emulator = m_Target as EmulatorTarget;

                            emulator.SetInputMapping(SDLInputs.eInputUp, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeUp));
                            emulator.SetInputMapping(SDLInputs.eInputDown, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeDown));
                            emulator.SetInputMapping(SDLInputs.eInputLeft, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeLeft));
                            emulator.SetInputMapping(SDLInputs.eInputRight, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeRight));
                            emulator.SetInputMapping(SDLInputs.eInputA, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeA));
                            emulator.SetInputMapping(SDLInputs.eInputB, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeB));
                            emulator.SetInputMapping(SDLInputs.eInputC, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeC));
                            emulator.SetInputMapping(SDLInputs.eInputStart, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(m_Config.KeycodeStart));
                        }


                        //  Load Rom
                        m_Target.LoadBinary(binaryFile);

                        //  Lookup and set initial breakpoints
                        for (int i = 0; i < m_Breakpoints.Count; i++)
                        {
                            m_Breakpoints[i].address = m_DebugSymbols.GetAddress(m_Breakpoints[i].filename, m_Breakpoints[i].line);
                            SetTargetBreakpoint(m_Breakpoints[i].address);

                            // Move any breakpoints on lines without addresses
                            Tuple<string, int, int> fileLine = m_DebugSymbols.GetFileLine(m_Breakpoints[i].address);
                            if (m_Breakpoints[i].line != fileLine.Item3)
                            {
                                //Line differs, backup and set real line
                                m_Breakpoints[i].originalLine = m_Breakpoints[i].line;
                                m_Breakpoints[i].line = fileLine.Item3;

                                if(m_Breakpoints[i].filename.Equals(m_CurrentlyEditingFile, StringComparison.OrdinalIgnoreCase))
                                {
                                    //Remove all breakpoint carets up to the real line
                                    for (int line = fileLine.Item2; line < fileLine.Item3; line++)
                                    {
                                        if (codeEditor.Document.BookmarkManager.IsMarked(line))
                                            codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, line));
                                    }

                                    //Set real breakpoint
                                    if (!codeEditor.Document.BookmarkManager.IsMarked(fileLine.Item3))
                                        codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, fileLine.Item3));
                                }
                            }
                        }

                        codeEditor.Refresh();

                        // Set watchpoints
                        foreach (uint address in m_Watchpoints)
                        {
                            m_Target.AddWatchpoint(address, address + 4);
                        }

                        //  Start
                        m_Target.Run();

                        //  profiling?
                        m_Profile = profilerEnabledMenuOptions.Checked;
                    }

                    m_State = State.kRunning;

                    statusLabel.Text = "Running...";

                    codeEditor.Document.ReadOnly = true;

                    // Reset the vdp status
                    m_VDPStatus.Reset();

                    //  Hide the build window
                    m_BuildLog.Hide();

                    StartDebugging();
                }
            }
        }

        private void Disassemble(uint fromAddr)
        {
            if(m_DebugSymbols != null)
            {
                m_Disassembly = new List<Tuple<uint, int, string>>();

                int lineNo = 0;
                uint address = fromAddr;
                uint size = 0;
                int textSize = 0;

                for(int i = 0; i < s_MaxDisassemblyLines; i++)
                {
                    string text = string.Empty;
                    size = m_Target.Disassemble(address, ref text);

                    if (size == 0)
                        break;

                    m_Disassembly.Add(new Tuple<uint, int, string>(address, lineNo, text));
                    textSize = text.Length;
                    lineNo++;
                    address += size;
                }

                m_DisassembledFrom = fromAddr;
                m_DisassembledTo = address;

                StringBuilder builder = new StringBuilder(textSize);

                foreach (var line in m_Disassembly)
                {
                    builder.Append(line.Item3);
                }

                m_DisassemblyText = builder.ToString();
            }
        }

        private void toggleBreakpoint_Click(object sender, EventArgs e)
        {
            int lineNo = codeEditor.ActiveTextAreaControl.Caret.Line;

            if (m_Breakpoints.Find(breakpoint => breakpoint.filename.Equals(m_CurrentlyEditingFile, StringComparison.OrdinalIgnoreCase) && breakpoint.line == lineNo) == null)
            {
                lineNo = SetBreakpoint(m_CurrentlyEditingFile, lineNo);

                if (!codeEditor.Document.BookmarkManager.IsMarked(lineNo))
                    codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, lineNo));
            }
            else
            {
                lineNo = RemoveBreakpoint(m_CurrentlyEditingFile, lineNo);

                if (codeEditor.Document.BookmarkManager.IsMarked(lineNo))
                    codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, lineNo));
            }

            codeEditor.Refresh();
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void stepIntoMenu_Click(object sender, EventArgs e)
        {
            //Step to next instruction (blocking)
            m_Target.Step();

            //Go to address
            uint currentPC = m_Target.GetPC();
            GoTo(currentPC);

            //Re-evaluate on next timer tick
            m_State = State.kRunning;
        }

        private void stepOverMenu_Click(object sender, EventArgs e)
        {
            //TODO: Breakpoints in disassembly mode
            if (m_SourceMode == SourceMode.Disassembly)
            {
                stepIntoMenu_Click(sender, e);
            }
            else
            {
                //Get current address
                uint currentPC = m_Target.GetPC();
                uint nextPC = currentPC;

                //Get current file/line
                Tuple<string, int, int> currentLine = m_DebugSymbols.GetFileLine((uint)currentPC);
                int nextLine = currentLine.Item3;

                //Determine if current instruction should be stepped into
                //TODO: Add instruction peek to DGen, determine by opcode
                string currentLineText = codeEditor.Document.GetText(codeEditor.Document.GetLineSegment(currentLine.Item3));
                Match match = Regex.Match(currentLineText, "\\s*?([a-zA-Z.]+)");
                if (match.Success)
                {
                    string opcode = match.Groups[1].ToString().ToUpper();

                    //Strip whitespace
                    Regex.Replace(opcode, @"\s+", "");

                    //Strip all after .
                    int dotPos = opcode.LastIndexOf(".");
                    if (dotPos >= 0)
                    {
                        opcode = opcode.Substring(0, dotPos);
                    }

                    if (kStepIntoInstrs.Contains(opcode))
                    {
                        stepIntoMenu_Click(sender, e);
                        return;
                    }
                }

                //Get total num lines
                //TODO: Verify current filename in editor matches emulator?
                int fileSizeLines = codeEditor.Document.TotalNumberOfLines;

                //Ignore lines with same address as current
                while (currentPC == nextPC)
                {
                    //Get next line
                    nextLine++;

                    //If next line is in another file, step into instead
                    if (nextLine > fileSizeLines)
                    {
                        stepIntoMenu_Click(sender, e);
                        return;
                    }

                    //Get address of next line
                    nextPC = m_DebugSymbols.GetAddress(currentLine.Item1, nextLine);
                }

                //Set breakpoint at next address
                m_Target.AddBreakpoint(nextPC);

                //Set StepOver mode
                m_BreakMode = BreakMode.kStepOver;
                m_StepOverBreakpoint = new Breakpoint(currentLine.Item1, nextLine, nextPC);

                //Run to StepOver breakpoint
                m_Target.Resume();
                m_State = State.kRunning;
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_State != State.kStopped)
            {
                if (m_Profile)
                {
                    uint totalCycles = 0;

                    //TODO: Profiler interface for all targets
                    if(m_Target is TargetDGen)
                    {
                        unsafe
                        {
                            int numInstructions = 0;
                            uint* profileResults = DGenThread.GetDGen().GetProfilerResults(&numInstructions);

                            m_ProfileResults = new List<ProfilerEntry>();

                            for (int i = 0; i < numInstructions; i++)
                            {
                                if (profileResults[i] > 0)
                                {
                                    ProfilerEntry entry = new ProfilerEntry();

                                    entry.address = (uint)i * sizeof(short);
                                    entry.hitCount = profileResults[i];
                                    Tuple<string, int, int> line = m_DebugSymbols.GetFileLine(entry.address);
                                    entry.cyclesPerHit = DGenThread.GetDGen().GetInstructionCycleCount(entry.address);
                                    entry.totalCycles = entry.cyclesPerHit * entry.hitCount;
                                    entry.filename = line.Item1;
                                    entry.line = line.Item3;

                                    m_ProfileResults.Add(entry);

                                    totalCycles += entry.totalCycles;
                                }
                            }
                        }

                        if (m_ProfileResults.Count > 0)
                        {
                            //Calcuate percentage cost
                            foreach (var entry in m_ProfileResults)
                            {
                                entry.percentCost = ((float)entry.totalCycles / (float)totalCycles);
                            }

                            //Sort by hit count
                            m_ProfileResults.Sort((a, b) => (int)(b.totalCycles - a.totalCycles));

                            m_ProfilerView.SetResults(m_ProfileResults);
                            m_ProfilerView.Show();
                        }
                    }
                }

                if(m_Target is EmulatorTarget)
                {
                    (m_Target as EmulatorTarget).Shutdown();
                }

                m_RegisterView.Hide();

                codeEditor.Document.MarkerStrategy.Clear();

                statusLabel.Text = "Stopped";

                m_State = State.kStopped;
                codeEditor.Document.ReadOnly = false;

                StopDebugging();

                //Restore original breakpoint lines
                for(int i = 0; i < m_Breakpoints.Count; i++)
                {
                    if(m_Breakpoints[i].originalLine > 0)
                    {
                        if (m_Breakpoints[i].filename.Equals(m_CurrentlyEditingFile, StringComparison.OrdinalIgnoreCase))
                        {
                            if (codeEditor.Document.BookmarkManager.IsMarked(m_Breakpoints[i].line))
                                codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, m_Breakpoints[i].line));
                            if (!codeEditor.Document.BookmarkManager.IsMarked(m_Breakpoints[i].originalLine))
                                codeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, m_Breakpoints[i].originalLine));
                        }

                        m_Breakpoints[i].line = m_Breakpoints[i].originalLine;
                        m_Breakpoints[i].originalLine = 0;
                    }
                }

                codeEditor.Refresh();
            }
        }

        private void breakMenu_Click(object sender, EventArgs e)
        {
            m_Target.Break();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_Target is EmulatorTarget)
            {
                (m_Target as EmulatorTarget).Shutdown();
            }

            if (m_Modified)
            {
                if (MessageBox.Show("Save changes?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Save();
                }
            }

            Settings.Default.WindowState = this.WindowState;

            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowLocation = this.Location;
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowLocation = this.RestoreBounds.Location;
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            Settings.Default.CRAMWindowVisible = m_CRAMViewer.Visible;
            Settings.Default.VDPStatusWindowVisible = m_VDPStatus.Visible;
            Settings.Default.ProfilerEnabled = profilerEnabledMenuOptions.Checked;

            Settings.Default.Save();
        }

        private void saveMenu_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save(bool saveAs = false)
        {
            if (m_SourceMode == SourceMode.Source)
            {
                if (m_CurrentlyEditingFile == null || saveAs)
                {
                    SaveFileDialog fileDialog = new SaveFileDialog();

                    fileDialog.Filter = "Source (*.asm;*.s;*.i)|*.ASM;*.S;*.I|All files (*.*)|*.*";
                    fileDialog.FilterIndex = 1;
                    fileDialog.RestoreDirectory = true;

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (!m_CurrentlyEditingFile.Equals(fileDialog.FileName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Filename changed in Save As dialog, add to project tree and open new filename
                            m_ProjectFiles.Add(fileDialog.FileName);
                            m_ProjectFiles.Sort();
                            PopulateFileView();
                            treeProjectFiles.ExpandAll();
                            OpenFile(fileDialog.FileName);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                m_SourceWatcher.EnableRaisingEvents = false;

                codeEditor.Encoding = Encoding.ASCII;
                codeEditor.SaveFile(m_CurrentlyEditingFile);

                m_SourceWatcher.Path = Path.GetDirectoryName(m_CurrentlyEditingFile);
                m_SourceWatcher.Filter = Path.GetFileName(m_CurrentlyEditingFile);
                m_SourceWatcher.EnableRaisingEvents = true;
            }

            m_Modified = false;
            UpdateTitle();
        }

        public void GoTo(string filename, int lineNumber)
        {
            if (m_CurrentlyEditingFile.ToLower() != filename.ToLower())
            {
                OpenFile(filename);
            }
            
            codeEditor.ActiveTextAreaControl.Caret.Line = lineNumber;

            this.Activate();
        }

        public void GoTo(uint address)
        {
            Tuple<string, int, int> currentLine = m_DebugSymbols.GetFileLine(address);

            string filename = currentLine.Item1;
            int lineNumberSymbols = currentLine.Item2;
            int lineNumberEditor = currentLine.Item3;

            if(lineNumberSymbols >= 0 && filename.Length > 0)
            {
                //Load file
                if (m_CurrentlyEditingFile.ToLower() != filename.ToLower())
                {
                    OpenFile(filename);
                }
            }
            else
            {
                //Display disassembly
                if(m_DisassemblyText == null || m_DisassemblyText.Length == 0 || address < m_DisassembledFrom || address >= m_DisassembledTo)
                {
                    Disassemble(address);
                }

                if (m_DisassemblyText != null && m_DisassemblyText.Length > 0)
                {
                    SetDisassemblyText(m_DisassemblyText);
                    Tuple<uint, int, string> lineNo = m_Disassembly.FirstOrDefault(entry => entry.Item1 == address);
                    if (lineNo != null)
                        lineNumberEditor = lineNo.Item2;
                    else
                        lineNumberEditor = 0;
                }
                else
                {
                    SetDisassemblyText("Disassembly unavailable");
                    lineNumberEditor = 0;
                }

                m_SourceMode = SourceMode.Disassembly;
            }

            int offset = codeEditor.Document.PositionToOffset(new TextLocation(0, lineNumberEditor));

            codeEditor.Document.MarkerStrategy.Clear();

            if (lineNumberEditor < codeEditor.Document.LineSegmentCollection.Count)
            {
                Marker marker = new Marker(offset, codeEditor.Document.LineSegmentCollection[lineNumberEditor].Length, MarkerType.SolidBlock, Color.Yellow, Color.Black);//selection.Offset, selection.Length, MarkerType.SolidBlock, Color.DarkRed, Color.White);
                codeEditor.Document.MarkerStrategy.AddMarker(marker);
                codeEditor.ActiveTextAreaControl.Caret.Line = lineNumberEditor;
                codeEditor.ActiveTextAreaControl.CenterViewOn(lineNumberEditor, -1);
            }
            else
            {
                codeEditor.ActiveTextAreaControl.Caret.Line = lineNumberEditor;
                codeEditor.ActiveTextAreaControl.CenterViewOn(0, -1);
            }

            codeEditor.ActiveTextAreaControl.Caret.Column = 0;
            codeEditor.ActiveTextAreaControl.Invalidate();
        }

        public void UpdateViewBuildLog(bool flag)
        {
            viewBuildLogMenu.Checked = flag;
        }

        public void UpdateVDPHelperMenu(bool flag)
        {
            vdpToolsRegistersMenu.Checked = flag;
        }

        public void UpdateViewCRAM(bool flag)
        {
            viewCRAMmenu.Checked = flag;
        }

        public void UpdateViewVDPStatus(bool flag)
        {
            viewVDPStatusMenu.Checked = flag;
        }

        public void UpdateViewProfiler(bool flag)
        {
            viewVDPStatusMenu.Checked = flag;
        }

        private void viewBuildLogMenu_Click(object sender, EventArgs e)
        {
            if (!viewBuildLogMenu.Checked)
                m_BuildLog.Show();
            else
            {
                m_BuildLog.Hide();
            }
        }

        private void configMenu_Click(object sender, EventArgs e)
        {
            ConfigForm configForm = new ConfigForm();

            configForm.StartPosition = FormStartPosition.CenterParent;

            configForm.targetList.SelectedIndex = configForm.targetList.FindString(m_Config.TargetName);
            configForm.asmPath.Text = m_Config.Asm68kPath;
            configForm.asmArgs.Text = m_Config.Asm68kArgs;
            configForm.emuResolution.SelectedIndex = m_Config.EmuResolution;
            configForm.emuRegion.SelectedIndex = m_Config.EmuRegion;
            configForm.autoOpenLastProject.Checked = m_Config.AutoOpenLastProject;

            configForm.inputUp.SelectedIndex = m_Config.KeycodeUp;
            configForm.inputDown.SelectedIndex = m_Config.KeycodeDown;
            configForm.inputLeft.SelectedIndex = m_Config.KeycodeLeft;
            configForm.inputRight.SelectedIndex = m_Config.KeycodeRight;
            configForm.inputA.SelectedIndex = m_Config.KeycodeA;
            configForm.inputB.SelectedIndex = m_Config.KeycodeB;
            configForm.inputC.SelectedIndex = m_Config.KeycodeC;
            configForm.inputStart.SelectedIndex = m_Config.KeycodeStart;

            configForm.modePAL.Checked = m_Config.Pal;
            configForm.modeNTSC.Checked = !m_Config.Pal;

            configForm.megaUSBPath.Text = m_Config.MegaUSBPath;

            configForm.listIncludes.Items.Clear();
            if(m_Config.Asm68kIncludePaths != null)
            {
                foreach(string include in m_Config.Asm68kIncludePaths)
                    configForm.listIncludes.Items.Add(include);
            }

            if (configForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_Config.TargetName = configForm.targetList.GetItemText(configForm.targetList.SelectedItem);
                m_Config.Asm68kPath = configForm.asmPath.Text;
                m_Config.Asm68kArgs = configForm.asmArgs.Text;
                m_Config.EmuResolution = configForm.emuResolution.SelectedIndex;
                m_Config.EmuRegion = configForm.emuRegion.SelectedIndex;
                m_Config.AutoOpenLastProject = configForm.autoOpenLastProject.Checked;
                m_Config.LastProject = m_ProjectFile;

                m_Config.KeycodeUp = configForm.inputUp.SelectedIndex;
                m_Config.KeycodeDown = configForm.inputDown.SelectedIndex;
                m_Config.KeycodeLeft = configForm.inputLeft.SelectedIndex;
                m_Config.KeycodeRight = configForm.inputRight.SelectedIndex;
                m_Config.KeycodeA = configForm.inputA.SelectedIndex;
                m_Config.KeycodeB = configForm.inputB.SelectedIndex;
                m_Config.KeycodeC = configForm.inputC.SelectedIndex;
                m_Config.KeycodeStart = configForm.inputStart.SelectedIndex;

                m_Config.Pal = configForm.modePAL.Checked;

                m_Config.MegaUSBPath = configForm.megaUSBPath.Text;

                m_Config.Asm68kIncludePaths = new string[configForm.listIncludes.Items.Count];
                configForm.listIncludes.Items.CopyTo(m_Config.Asm68kIncludePaths, 0);

                m_Config.Save();

                Console.WriteLine(configForm.asmPath.Text);
                Console.WriteLine(configForm.asmArgs.Text);

                //Recreate target
                m_Target = TargetFactory.Create(m_Config.TargetName);

                //Rebuild directory view
                PopulateFileView();
            }
        }

        private void vDPRegistersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!vdpToolsRegistersMenu.Checked)
            {
                m_VDPRegs.Show();
                vdpToolsRegistersMenu.Checked = true;
            }
            else
            {
                m_VDPRegs.Hide();
                vdpToolsRegistersMenu.Checked = false;
            }
        }

        private void OpenProject(string filename)
        {
            if(System.IO.File.Exists(filename))
            {
                // Set project paths
                m_ProjectFile = filename;
                m_PathToProject = Path.GetDirectoryName(filename);
                m_ProjectName = Path.GetFileNameWithoutExtension(filename);
                m_FileToAssemble = filename;

                // Open first file
                OpenFile(filename);

                // Populate tree view
                PopulateFileView();
                treeProjectFiles.ExpandAll();
            }
        }

        private void WithFileAccessRetry(Action action, int timeoutMs = 5000)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    action();
                    return;
                }
                catch (IOException e)
                {
                    // access error
                    if (e.HResult != -2147024864)
                        throw;
                }
            }
            throw new Exception("Failed to access file within allotted time");
        }

        private void OnSourceChanged(object source, FileSystemEventArgs e)
        {
            if (m_SourceMode == SourceMode.Source)
            {
                if (System.Threading.Monitor.TryEnter(m_WatcherCritSec))
                {
                    // Disable further checks
                    m_SourceWatcher.EnableRaisingEvents = false;

                    // Open file not thread safe, invoke main thread action to open
                    this.Invoke(new Action(() =>
                    {
                    // Did any contents actually change?
                    string diskContents = null;

                        WithFileAccessRetry(() =>
                        {
                            diskContents = System.IO.File.ReadAllText(m_CurrentlyEditingFile);
                        });

                        if (diskContents != codeEditor.Document.TextContent)
                        {
                        // Ask user
                        DialogResult dialogResult = MessageBox.Show(this, m_CurrentlyEditingFile + Environment.NewLine + Environment.NewLine + "This file has been modified by an another program." + Environment.NewLine + Environment.NewLine + "Do you want to reload it?", "Reload", MessageBoxButtons.YesNo);

                            if (dialogResult == DialogResult.Yes)
                            {
                                OpenFile(m_CurrentlyEditingFile);
                            }
                        }
                    }));

                    // Re-enable checks
                    m_SourceWatcher.EnableRaisingEvents = true;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog pathSelect = new OpenFileDialog();

            pathSelect.Filter = "ASM|*.s;*.asm;*.68k;*.i";

            if (pathSelect.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenProject(pathSelect.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void UpdateTitle()
        {
            if (m_CurrentlyEditingFile != null && m_CurrentlyEditingFile.Length > 0)
            {
                if (m_Modified)
                    this.Text = "MDStudio - " + m_CurrentlyEditingFile + "*";
                else
                    this.Text = "MDStudio - " + m_CurrentlyEditingFile;
            }
            else
            {
                if (m_Modified)
                    this.Text = "MDStudio - *";
                else
                    this.Text = "MDStudio";
            }
        }
        private void documentChanged(object sender, EventArgs e)
        {
            m_Modified = true;

            UpdateTitle();

            ClearSearch();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeProjectFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(System.IO.Path.GetExtension(treeProjectFiles.SelectedNode.Name).Length > 0)
            {
                GoTo(treeProjectFiles.SelectedNode.Name, 0);
            }
        }

        private void searchSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SymbolView dialog = new SymbolView(this, m_DebugSymbols);
            dialog.ShowDialog(this);
        }

        IEnumerable<TreeNode> Collect(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach (var child in Collect(node.Nodes))
                    yield return child;
            }
        }

        private void searchFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> filenames = new List<string>();
            foreach(TreeNode node in Collect(treeProjectFiles.Nodes))
            {
                if (System.IO.Path.GetExtension(node.Name).Length > 0)
                {
                    filenames.Add(node.Name);
                }
            }

            FileView dialog = new MDStudio.FileView(this, filenames);
            dialog.ShowDialog(this);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_Config.LastProject = m_ProjectFile;
            m_Config.Save();
        }

        private void runMegaUSB_Click(object sender, EventArgs e)
        {
            string binaryFile = m_PathToProject + @"\" + m_ProjectName + ".bin";

            if(File.Exists(binaryFile) && m_Config.MegaUSBPath != null && File.Exists(m_Config.MegaUSBPath))
            {
                try
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = m_Config.MegaUSBPath;
                    proc.StartInfo.WorkingDirectory = m_PathToProject + @"\";
                    proc.StartInfo.Arguments = binaryFile;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();
                }
                catch
                {
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if(Settings.Default.WindowLocation != null)
            {
                this.Location = Settings.Default.WindowLocation;
            }

            if(Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
            }

            if(Settings.Default.WindowState != null)
            {
                this.WindowState = Settings.Default.WindowState;
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
            }

            profilerEnabledMenuOptions.Checked = Settings.Default.ProfilerEnabled;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {

        }

        private void MainForm_Enter(object sender, EventArgs e)
        {
            if (m_Target != null && m_State != State.kStopped)
            {
                if(m_Target is EmulatorTarget)
                {
                    (m_Target as EmulatorTarget).BringToFront();
                }
            }
        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void viewCRAMBtn_Click(object sender, EventArgs e)
        {
            if (!viewCRAMmenu.Checked)
                m_CRAMViewer.Show();
            else
                m_CRAMViewer.Hide();
        }

        private void viewVDPStatusMenu_Click(object sender, EventArgs e)
        {
            if (!viewVDPStatusMenu.Checked)
                m_VDPStatus.Show();
            else
                m_VDPStatus.Hide();
        }

        private void ClearSearch()
        {
            bool requestUpdate = m_SearchMarkers.Count>0;

            foreach (Marker marker in m_SearchMarkers)
            {
                codeEditor.Document.MarkerStrategy.RemoveMarker(marker);
            }

            m_SearchMarkers.Clear();
            m_SearchResults.Clear();
            m_SearchIndex = -1;

            if(requestUpdate)
            {
                codeEditor.Refresh();
            }
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForm search = new SearchForm();

            if(search.ShowDialog() == DialogResult.OK)
            {
                bool firstFind = true;

                ClearSearch();

                if (search.searchString.Text.Length > 0)
                {
                    Regex rx = new Regex(search.checkMatchCase.Checked ? search.searchString.Text : "(?i)" + search.searchString.Text);
                    foreach (Match match in rx.Matches(codeEditor.Document.TextContent))
                    {
                        TextLocation matchLocation = codeEditor.Document.OffsetToPosition(match.Index);

                        Marker marker = new Marker(match.Index, match.Length, MarkerType.SolidBlock, Color.Orange, Color.Black);
                        codeEditor.Document.MarkerStrategy.AddMarker(marker);

                        m_SearchMarkers.Add(marker);
                        m_SearchResults.Add(matchLocation);

                        if (firstFind)
                        {
                            m_SearchIndex++;
                            if (matchLocation.Line >= codeEditor.ActiveTextAreaControl.Caret.Line)
                            {
                                codeEditor.ActiveTextAreaControl.Caret.Position = matchLocation;
                                codeEditor.ActiveTextAreaControl.CenterViewOn(matchLocation.Line, -1);
                                firstFind = false;
                            }
                        }
                    }
                    Console.WriteLine("search: " + search.searchString);
                }
            }
        }

        private void searchNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_SearchResults.Count>0 && (m_SearchIndex+1)< m_SearchResults.Count)
            {
                m_SearchIndex++;

                codeEditor.ActiveTextAreaControl.Caret.Position = m_SearchResults[m_SearchIndex];
                codeEditor.ActiveTextAreaControl.CenterViewOn(m_SearchResults[m_SearchIndex].Line, -1);
            }
        }

        private void searchPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_SearchResults.Count > 0 && (m_SearchIndex - 1) >= 0)
            {
                m_SearchIndex--;

                codeEditor.ActiveTextAreaControl.Caret.Position = m_SearchResults[m_SearchIndex];
                codeEditor.ActiveTextAreaControl.CenterViewOn(m_SearchResults[m_SearchIndex].Line, -1);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void profileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void profilerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(viewVDPStatusMenu.Checked)
                m_ProfilerView.Show();
            else
                m_ProfilerView.Hide();
        }

        private void searchReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForm search = new SearchForm(true);

            if (search.ShowDialog() == DialogResult.OK)
            {
                ClearSearch();

                m_ReplaceString = search.replaceString.Text;

                if (search.searchString.Text.Length > 0)
                {
                    Regex rx = new Regex(search.checkMatchCase.Checked ? search.searchString.Text : "(?i)" + search.searchString.Text);

                    //  should do one by one ideally
                    codeEditor.Document.TextContent = rx.Replace(codeEditor.Document.TextContent, m_ReplaceString);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();

            about.ShowDialog();
        }

        public void OnKeyDown(int vkCode)
        {
            if(m_State == State.kRunning)
            {
                if(m_Target is EmulatorTarget)
                {
                    (m_Target as EmulatorTarget).SendKeyPress(vkCode, 1);
                }
            }
        }

        public void OnKeyUp(int vkCode)
        {
            if (m_State == State.kRunning)
            {
                if (m_Target is EmulatorTarget)
                {
                    (m_Target as EmulatorTarget).SendKeyPress(vkCode, 0);
                }
            }
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToForm gotoForm = new GoToForm(GoToForm.Type.Line);

            if(gotoForm.ShowDialog() == DialogResult.OK)
            {
                int lineNumber;

                if(int.TryParse(gotoForm.textLineNumber.Text, out lineNumber))
                {
                    //Text editor is 0-based
                    codeEditor.ActiveTextAreaControl.Caret.Line = lineNumber - 1;
                }
            }
        }

        private void goToAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToForm gotoForm = new GoToForm(GoToForm.Type.Address);

            if (gotoForm.ShowDialog() == DialogResult.OK)
            {
                uint address;

                if (uint.TryParse(gotoForm.textLineNumber.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
                {
                    GoTo(address);
                }
            }
        }

        private void ResetDocument()
        {
            codeEditor.Document.DocumentChanged -= documentChanged;

            m_FileToAssemble = null;
            m_CurrentlyEditingFile = null;
            m_Modified = false;
            codeEditor.Document.TextContent = null;
            m_SourceWatcher = null;
            codeEditor.Refresh();
            codeEditor.Document.UndoStack.ClearAll();
            codeEditor.Document.BookmarkManager.Clear();
            UpdateTitle();
            treeProjectFiles.Nodes.Clear();
            m_ProjectFiles.Clear();

            codeEditor.Document.DocumentChanged += documentChanged;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_State != State.kStopped)
                return;

            if (m_Modified)
            {
                if (MessageBox.Show("Save changes?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Save();
                }
            }

            ResetDocument();
        }

        private void fooToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if UMDK_SUPPORT
            string binaryFile = m_PathToProject + @"\" + m_ProjectName + ".bin";

            m_UMDK.Open();
            m_UMDK.WriteFile(binaryFile);
            m_UMDK.Close();
#endif
        }

        private void addWatchpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToForm gotoForm = new GoToForm(GoToForm.Type.Address);
            
            if (gotoForm.ShowDialog() == DialogResult.OK)
            {
                uint address = Convert.ToUInt32(gotoForm.textLineNumber.Text, 16);

                //if (uint.TryParse(gotoForm.textLineNumber.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
                {
                    if(address > 0)
                    {
                        m_Watchpoints.Add(address);

                        if (m_State != State.kStopped)
                        {
                            m_Target.AddWatchpoint(address, address + 3);
                        }
                    }
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
#if UMDK_SUPPORT
            string binaryFile = m_PathToProject + @"\" + m_ProjectName + ".bin";

            m_UMDK.Open();
            m_UMDK.WriteFile(binaryFile);
            m_UMDK.Close();
#endif
        }

        private void addLogpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToForm gotoForm = new GoToForm(GoToForm.Type.Address);

            if (gotoForm.ShowDialog() == DialogResult.OK)
            {
                uint address;

                if (uint.TryParse(gotoForm.textLineNumber.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
                {
                    m_Watchpoints.Add(address);

                    if (m_State != State.kStopped)
                    {
                        m_Target.AddWatchpoint(address, address + 3);
                    }

                    m_BreakMode = BreakMode.kLogPoint;
                }
            }
        }

        private void pauseResumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_State == State.kRunning)
            {
                m_Target.Break();
                m_State = State.kPaused;
            }
            else if(m_State == State.kPaused)
            {
                m_Target.Resume();
                m_State = State.kRunning;
            }
        }

        private void softResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_Target is EmulatorTarget)
            {
                (m_Target as EmulatorTarget).SoftReset();
            }
            else
            {
                m_Target.Reset();
            }
        }

        private void breakpointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_BreakpointView = new BreakpointView(this, m_DebugSymbols);

            if(m_DebugSymbols != null)
            {
                //TODO: Breakpoint view by file/line, not address
                foreach (Breakpoint breakpoint in m_Breakpoints)
                {
                    m_BreakpointView.SetBreakpoint(m_DebugSymbols.GetAddress(breakpoint.filename, breakpoint.line));
                }
            }

            m_BreakpointView.Show();
        }
    }
}
