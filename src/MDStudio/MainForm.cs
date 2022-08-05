//#define UMDK_SUPPORT

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
using MDStudio.Debugging;
using MDStudio.Tools;
using MDStudio.Debug;
using static MDStudio.Themes;

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

        #region variables
        private readonly Timer m_Timer = new Timer();
        private Project m_Project;
        private DigitalRune.Windows.TextEditor.TextEditorControl currentCodeEditor;
        private string m_ProjectFile;
        private string m_PathToProject;
        private string m_ProjectName;
        private string m_FileToAssemble;        // File to assemble
        private string m_CurrentlyEditingFile;  // Currently editing source file
        private List<string> m_ProjectFiles;
        private bool m_AlreadyBuilt;
        public Target m_Target;
        public ISymbols m_DebugSymbols;
        private string m_DisassemblyText;
        private List<Tuple<uint, int, string>> m_Disassembly;
        private static int s_MaxDisassemblyLines = 200;
        private uint m_DisassembledFrom = 0;
        private uint m_DisassembledTo = 0;
        private Theme CurrentTheme = Theme.Light;
        private RegisterView m_RegisterView;
        private MemoryView m_MemoryView;
        private BuildLog m_BuildLog;
        private CRamViewer m_CRAMViewer;
        private VDPStatusWindow m_VDPStatus;
        private ProfilerView m_ProfilerView;
        private BreakpointView m_BreakpointView;

        private Config m_Config;

        private VDPRegs m_VDPRegs;
        private VdpPatternView m_VDPViewer;
        private SoundOptions m_SoundOptions;

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
        private int m_Emulator_Volume = 100;

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

        private bool shouldAlwaysRunPostBuildCommand = false;

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

        #endregion
        
        public MainForm()
        {
            InitializeComponent();
            m_Config = new Config();
            m_Config.Read();

            m_ErrorMarkers = new List<Marker>();
            m_SearchMarkers= new List<Marker>();
            m_SearchResults = new List<TextLocation>();
            m_Breakpoints = new List<Breakpoint>();
            m_Watchpoints = new List<uint>();

            m_VDPRegs = new VDPRegs(this);

            m_VDPViewer = new VdpPatternView(this);

            m_SoundOptions = new SoundOptions();
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

            m_MemoryView = new MemoryView();

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

        private void OpenFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                // Ignore document changed events
                bool watchingEvents = (m_SourceWatcher == null) || m_SourceWatcher.EnableRaisingEvents;
                m_SourceWatcher = null;
                
                // this serves as the name of the tab AND a key for the tabpages in the tabcontrol
                // as well as the code editor control within the tabpage.
                string tabText = Path.GetFileName(filename);

                // determine if the tab is already open
                bool tabExists = DocumentTabs.TabPages.ContainsKey(filename);

                // if tab doesn't exist, create a new tabpage with name and key
                if (!tabExists)
                {
                    DocumentTabs.TabPages.Add(filename, tabText);
                }

                // get the tab
                int index = DocumentTabs.TabPages.IndexOfKey(filename);
                var page = DocumentTabs.TabPages[index];

                // get the code editor control from the tabpage or create a new one
                DigitalRune.Windows.TextEditor.TextEditorControl codeEditor = null;

                // if this is a new tabpage, create a new code editor and set options
                if (!tabExists)
                {
                    codeEditor = new DigitalRune.Windows.TextEditor.TextEditorControl();
                    page.Controls.Add(codeEditor);
                    
                    codeEditor.Name = tabText;
                    codeEditor.IsIconBarVisible = true;
                    codeEditor.AutoScroll = true;
                    codeEditor.Dock = DockStyle.Fill;
                    codeEditor.ShowVRuler = false;

                    string theme = "ASM68K";
                    if (m_Config.Theme == Theme.Dark)
                    {
                        theme += "-Dark";
                    }
                    // Set the syntax-highlighting for ASM68k
                    codeEditor.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter(theme);

                    codeEditor.Document.DocumentChanged -= documentChanged;
                    codeEditor.LoadFile(filename);
                }
                else
                {
                    // get the editor that is already inside the tabpage (I wish there was an easier way to get this)
                    codeEditor = (DigitalRune.Windows.TextEditor.TextEditorControl)page.Controls.Find(tabText, true).First();
                }
                
                // set the current code editor
                currentCodeEditor = codeEditor;

                // select the new page
                DocumentTabs.SelectedTab = page;

                // Reset undo state
                m_Modified = false;
                undoMenu.Enabled = false;

                // Load file
                m_CurrentlyEditingFile = filename;


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
            currentCodeEditor.Document.DocumentChanged -= documentChanged;

            // Reset undo state
            m_Modified = false;
            undoMenu.Enabled = false;

            // Load file
            m_CurrentlyEditingFile = "Disassembly";
            currentCodeEditor.Text = text;

            // Set title bar text
            this.Text = "MDStudio - " + "Disassembly";

            // Refresh
            currentCodeEditor.Refresh();
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
                if (m_CRAMViewer.Visible || m_VDPViewer.Visible)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        uint rgb = (m_Target as EmulatorTarget).GetColor(i);
                        if (m_CRAMViewer.Visible)
                        {
                            m_CRAMViewer.SetColor(i, rgb);
                        }

                        if (m_VDPViewer.Visible)
                        {
                            // TODO: get color to update the vdp palette?
                            m_VDPViewer.SetPaletteEntry(i, rgb);
                        }
                    }

                    UpdateVdpRAM();
                }
            }
        }

        private void UpdateVdpRAM()
        {
            // TODO:
            // vdp viewer is visible
            if (true)
            {
                // get vram from emulator
                // pass vram data to vdp viewer
                // loop through and create tiles
                unsafe
                {
                    var vram = DGenThread.GetDGen().GetVRAM();

                    /*for (int tile = 0; tile < 2048; tile++) //2048
                    {
                        Bitmap image = new Bitmap(8, 8);
                        for (int pixely = 0; pixely < 8; pixely++)
                        {
                            // each byte is 2 pixels
                            for (int pixelx = 0; pixelx < 4; pixelx++)
                            {
                                // get color value
                                byte twoPixels = vram[tile * pixelx * pixely];
                                byte pix1 = ((byte)((byte)(twoPixels & 0xF0) >> 4));
                                byte pix2 = (byte)(twoPixels & 0x0F);

                                var color1 = m_VDPViewer.GetColor(pix1);
                                var color2 = m_VDPViewer.GetColor(pix2);

                                // set color
                                image.SetPixel(pixelx*2, pixely, color1);
                                image.SetPixel((pixelx*2)+1, pixely, color2);
                            }
                        }
*/
                        m_VDPViewer.SetVRam(vram);
                    //}
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
                    //m_Target.SetVolume(0);
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
                if (m_SoundOptions.Visible)
                {
                    m_Emulator_Volume = m_SoundOptions.Volume;
                }

                //m_Target.SetVolume(m_Emulator_Volume);

                UpdateCRAM();
                if (m_MemoryView.Visible)
                {
                    byte[] memBuffer = new byte[0xFFFF];
                     m_Target.ReadMemory(0xFFFF0000, 0xFFFF, memBuffer);
                    m_MemoryView.SetRamMemory(memBuffer);
                }
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
                                if (m_Config.AssemblerIncludePaths != null)
                                {
                                    foreach (string includePath in m_Config.AssemblerIncludePaths)
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
            if (m_Project == null)
                return;

            //Scan for includes
            // Disabling for now. Cannot load very large, complicated projects. 
            // I let it sit for about 8 hours and it still couldn't complete loading. 
            // Consider building a project file format to load instead. 
            if (false)
            {
                m_ProjectFiles = ScanIncludes(m_PathToProject, m_ProjectFile);
            }
            else if (m_ProjectFiles == null || m_ProjectFiles.Count == 0)
            {

                var files = m_Project.SourceFiles?.ToList();
                
                m_ProjectFiles = new List<string>();
                m_ProjectFiles.Add($"{m_PathToProject}\\{m_Project.MainSourceFile}");

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        m_ProjectFiles.Add($"{m_PathToProject}\\{file}");
                    }
                }
            }

            if (m_ProjectFiles != null)
            {
                //Add current file and sort
                //m_ProjectFiles.Add(m_CurrentlyEditingFile);
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
        }

        private void undoMenu_Click(object sender, EventArgs e)
        {
            currentCodeEditor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentCodeEditor.Redo();
        }

        private void codeEditor_DocumentChanged(object sender, DocumentEventArgs e)
        {
            if (currentCodeEditor.Document.UndoStack.CanUndo)
            {
                undoMenu.Enabled = true;
            }
            else
            {
                undoMenu.Enabled = false;
            }

            if (currentCodeEditor.Document.UndoStack.CanRedo)
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

        private int PreBuild()
        {
            int returnValue = 0;
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (sender, e) => e.Result = Build();
            worker.RunWorkerCompleted += (sender, e) => { returnValue = (int)e.Result; };
            worker.RunWorkerAsync();
            return returnValue;
        }

        /// <summary>
        /// Runs pre/post commands
        /// </summary>
        /// <param name="script"></param>
        private void RunScript(string script)
        {
            string[] commands = script.Split('\n');
            int timeout = 60 * 1000 * 1000;
           
            StringBuilder processStandardOutput = new StringBuilder();
            StringBuilder processErrorOutput = new StringBuilder();

            using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
            using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.WorkingDirectory = m_PathToProject + @"\";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    using (StreamWriter sw = process.StandardInput)
                    {
                        foreach (var command in commands)
                        {
                            if (!command.StartsWith("REM") || !String.IsNullOrWhiteSpace(command))
                            {
                                if (sw.BaseStream.CanWrite)
                                {
                                    sw.WriteLine(command);
                                }
                            }
                        }
                    }

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

                string[] output;
                if (processErrorOutput.Length > 0)
                    output = processErrorOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                else
                    output = processStandardOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);


                foreach (string line in output)
                {
                    Action build_addraw = () =>
                    {
                        m_BuildLog.AddRaw(line);
                    };
                    BeginInvoke(build_addraw);
                }
            }                            
        }

        private int Build()
        {          
            Stopwatch sw = Stopwatch.StartNew();
            string assemblerPath = m_Project.Assembler == Assembler.AS ? m_Config.AsPath : m_Config.Asm68kPath;

            if (assemblerPath == null || assemblerPath.Length == 0)
            {
                Action messageAction = () =>
                {
                    MessageBox.Show("Assembler Path not set\nPlease set it in the Config menu.");
                    configMenu_Click(this, null);
                };
                BeginInvoke(messageAction);

                return 0;
            }

            if (!File.Exists(assemblerPath))
            {
                MessageBox.Show("Cannot find '" + assemblerPath + "'");
                return 0;
            }

            Action action = () =>
            {
                m_BuildLog.Clear();
                if (!m_BuildLog.Visible)
                    m_BuildLog.Show();
                
                m_BuildLog.SelectLogTab();                
            };

            BeginInvoke(action);

            bool use_asm68k = m_Project.Assembler == Assembler.Asm68k ? true : false;
            string mainFileName = Path.GetFileNameWithoutExtension(m_Project.MainSourceFile);

            // need to delete some 
            var lstFile = $"{m_PathToProject}\\" + (use_asm68k ? $"{m_Project.Name}.list" :$"{mainFileName}.lst");
            var hFile = $"{m_PathToProject}\\{mainFileName}.h";
            var pFile = $"{m_PathToProject}\\{mainFileName}.p";           
            string binaryFile = $"{m_PathToProject}\\" + (use_asm68k ? $"{m_Project.Name}.bin" : $"{mainFileName}.bin");
            string symbolFile = $"{m_PathToProject}\\" + (use_asm68k ? $"{m_Project.Name}.symb" : $"{mainFileName}.map");

            File.Delete(symbolFile);
            File.Delete(binaryFile);
            File.Delete(lstFile);

            // these are specific to AS
            if (!use_asm68k)
            {
                File.Delete(hFile);
                File.Delete(pFile);
            }

            Console.WriteLine("compile");
            statusLabel.Text = "Building...";
            StringBuilder processStandardOutput = new StringBuilder();
            StringBuilder processErrorOutput = new StringBuilder();

            int errorCount = 0;

            if (m_Project.PreBuildScript != String.Empty)
                RunScript(m_Project.PreBuildScript);

            try
            {
                int timeout = 60 * 1000 * 1000;

                using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
                using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
                {
                    using (Process process = new Process())
                    {
                        string includeArgs = "";

                        if (use_asm68k)
                        {
                            BuildAsm68k(process, includeArgs);
                        }
                        else if (!use_asm68k)
                        {
                            BuildAsAssembler(process, includeArgs);
                        }

                        Console.WriteLine("Assembler: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);



                        Action buildLogRefreshAction = () =>
                        {
                            m_BuildLog.AddRaw(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                            m_BuildLog.Refresh();
                        };

                        BeginInvoke(buildLogRefreshAction);

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

                            // I don't think this is needed here.
                            //process.Start();

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

            if (processErrorOutput.Length > 0)
                output = processErrorOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            else
                output = processStandardOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (Marker marker in m_ErrorMarkers)
            {
                currentCodeEditor.Document.MarkerStrategy.RemoveMarker(marker);
            }

            m_ErrorMarkers.Clear();

            foreach (string line in output)
            {
                Console.WriteLine(line);

                // remove some of the whitespace and the "console line stuff"
                int cmdPromptLength = $"{m_PathToProject}>".Length;
                string logline = line.Replace('\b',' ').Trim();

                if(!String.IsNullOrWhiteSpace(logline) && logline.Length >= cmdPromptLength && logline.Contains(m_PathToProject))
                {
                    logline = logline.Remove(0, cmdPromptLength);
                }

                Action build_addraw = () => {
                    m_BuildLog.AddRaw(logline);
                };
                BeginInvoke(build_addraw);

                string patternError = @"([\w:\\.]*)\((\d+)\) : Error : (.+)";

                if (!use_asm68k)
                {
                    patternError = @"> > >([\w:\\.]*)\((\d+)\): error (.+)";
                }
              
                Match matchError = Regex.Match(logline, patternError);

                if (matchError.Success)
                {
                    int lineNumber;

                    int.TryParse(matchError.Groups[2].Value, out lineNumber);

                    string filename = matchError.Groups[1].Value;
                    Action buildLogError = () =>
                    {
                        m_BuildLog.AddError(filename, lineNumber, matchError.Groups[3].Value);
                    };

                    BeginInvoke(buildLogError);

                    System.Diagnostics.Debug.WriteLine("Error in '" + matchError.Groups[1].Value + "' (" + matchError.Groups[2].Value + "): " + matchError.Groups[3].Value);
                    errorCount++;

                    //  Mark the line
                    if (matchError.Groups[1].Value == m_FileToAssemble)
                    {
                        int offset = currentCodeEditor.Document.PositionToOffset(new TextLocation(0, lineNumber - 1));
                        Marker marker = new Marker(offset, currentCodeEditor.Document.LineSegmentCollection[lineNumber - 1].Length, MarkerType.SolidBlock, Color.DarkRed, Color.Black);
                        Action codedEditorMark = () =>
                        {
                            currentCodeEditor.Document.MarkerStrategy.AddMarker(marker);
                            m_ErrorMarkers.Add(marker);
                        };
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(errorCount + " Error(s)");

            Action codeEditorRefreshAction = () =>
            {
                currentCodeEditor.Refresh();                
            };

            BeginInvoke(codeEditorRefreshAction);            

            sw.Stop();
            string buildTime = $"Build Time: {sw.Elapsed.Minutes}:{sw.Elapsed.Seconds}:{sw.Elapsed.Milliseconds}";

            if (errorCount > 0)
            {
                statusLabel.Text = errorCount + " Error(s) " + buildTime;
            }
            else if (((use_asm68k && !File.Exists(binaryFile) || !File.Exists(symbolFile)) || (!use_asm68k && !File.Exists(pFile))))
            {
                statusLabel.Text = "Build error, no output files generated ";
                statusLabel.Text += use_asm68k ? $"{mainFileName}.bin exists: {File.Exists(binaryFile)}, {mainFileName}.symb exists: {File.Exists(symbolFile)}" : $"{mainFileName}.p exists: {File.Exists(pFile)}, {mainFileName}.map exists: {File.Exists(symbolFile)}";
                statusLabel.Text += " " + buildTime;
                errorCount++;
            }
            else
            {
                //Read symbols
                try
                {
                    // Need to initialize the symbol data before read
                    if (m_DebugSymbols == null)
                    {
                        if (m_Project.Assembler == Assembler.Asm68k)
                        {
                            m_DebugSymbols = new Asm68kSymbols();
                        }
                        else
                        {
                            m_DebugSymbols = new AsSymbols();
                        }
                    }

                    // make sure files are all uppercase and have '/' instead of '\' for comparisons sake
                    var filesToExclude = m_Project.FilesToExclude?.Select(e => $"{m_PathToProject}\\{e}".ToUpper().Replace("/", "\\"));

                    m_DebugSymbols.Read(symbolFile, filesToExclude?.ToArray());
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine("Symbol parse error - malformed symbol file: {0}", exception.Message);

                    if (m_Project.Assembler == Assembler.Asm68k)
                    {
                        m_DebugSymbols = new Asm68kSymbols();
                    }
                    else
                    {
                        m_DebugSymbols = new AsSymbols();
                    }
                }                

                statusLabel.Text = "Build ok! " + buildTime;
                m_AlreadyBuilt = true;
            }

            if(shouldAlwaysRunPostBuildCommand)
            {
                if (m_Project.PostBuildScript != String.Empty)
                    RunScript(m_Project.PostBuildScript);
            }
            else
            {
                if (m_Project.PostBuildScript != String.Empty && errorCount == 0)
                    RunScript(m_Project.PostBuildScript);
            }

            if (errorCount > 0)
            {
                action = () => m_BuildLog.SelectErrorTab();

                BeginInvoke(action);
            }

            return errorCount;
        }

        private void BuildAsAssembler(Process process,string includeArgs)
        {
            /*if (m_Config.AssemblerIncludePaths != null && m_Config.AssemblerIncludePaths.Length > 0)
            {
                foreach (string include in m_Config.AssemblerIncludePaths)
                {
                    includeArgs += "/j " + include + "\\* "; // /j for include files
                }
            }*/

            process.StartInfo.FileName = m_Config.AsPath;
            process.StartInfo.WorkingDirectory = m_PathToProject + @"\";
            // default ags:
            // -xx : Level 2 for detailed error messages
            // -q : suppress messages
            // -c : variables will be written in a format which permits an easy integration into a C-source file. The extension of the file is H
            // -A : stores the list of global symbols in another, more compact form
            // -L : writes assembler listing into a file
            // -g MAP : This switch instructs AS to create an additional file that contains debug information for the program
            process.StartInfo.Arguments = @"-xx -n -c -A -L -g MAP " + m_Project.AdditionalArguments + " \"" + m_FileToAssemble + "\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
        }

        private void BuildAsm68k(Process process, string includeArgs)
        {
            if (m_Config.AssemblerIncludePaths != null && m_Config.AssemblerIncludePaths.Length > 0)
            {
                foreach (string include in m_Config.AssemblerIncludePaths)
                {
                    includeArgs += "/j " + include + "\\* ";
                }
            }

            var filenameWoExt = Path.GetFileNameWithoutExtension(m_FileToAssemble);
            process.StartInfo.FileName = m_Config.Asm68kPath;
            process.StartInfo.WorkingDirectory = m_PathToProject + @"\";
            process.StartInfo.Arguments = $"/p /c /zd {m_Project.AdditionalArguments} {includeArgs} \"{m_FileToAssemble}\", \"{m_ProjectName}.bin\", \"{m_ProjectName}.symb\", \"{m_ProjectName}.list\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
        }

        private void compileMenu_Click(object sender, EventArgs e)
        {
            Save();
            PreBuild();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (s, ev) => Run();
            //worker.RunWorkerCompleted += (s, ev) => { };
            worker.RunWorkerAsync();           
        }

        private void Run()
        {            
            if (m_State == State.kDebugging)
            {               
                Action action = () =>
                {
                    currentCodeEditor.Document.MarkerStrategy.Clear();
                    currentCodeEditor.ActiveTextAreaControl.Invalidate();
                    currentCodeEditor.Refresh();
                };

                BeginInvoke(action);

                m_Target.Resume();

                statusLabel.Text = "Running...";

                if (m_Target is EmulatorTarget emulator)
                {
                    emulator.BringToFront();
                }

                m_State = State.kRunning;
            }
            else if(m_State == State.kStopped)
            {
                Action action = () =>
                {
                    Save();
                };
                
                Invoke(action);

                // if we have already compiled, no reason to do it again.
                // this should speed up for simple Runs
                if (m_AlreadyBuilt || Build() == 0)
                {
                    //  Show tools windows first, so emu window gets foreground focus

                    action = () =>
                    {
                        try
                        {
                            m_RegisterView.Show();
                        }
                        catch (Exception e)
                        {
                            m_RegisterView = new RegisterView();
                            m_RegisterView.Show();
                        }

                        memoryViewerToolStripMenuItem.Enabled = true;
                    };

                    BeginInvoke(action);                    

                    if (m_BreakpointView != null)
                    {
                        action = () =>
                        {
                            m_BreakpointView.UpdateSymbols(m_DebugSymbols);
                        };

                        BeginInvoke(action);
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

                        action = () =>
                        {
                            if (m_Target is EmulatorTarget)
                            {
                                (m_Target as EmulatorTarget).Initialise(resolution.Item1, resolution.Item2, this.Handle, m_Config.Pal, kRegions[m_Config.EmuRegion].Item1);
                            }

                            //Set input mappings
                            if (m_Target is EmulatorTarget)
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

                            string binaryFile = $"{m_PathToProject}\\" + (m_Project.Assembler == Assembler.Asm68k ? $"{m_Project.Name}.bin" : $"{Path.GetFileNameWithoutExtension(m_Project.MainSourceFile)}.bin");

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

                                    if (m_Breakpoints[i].filename.Equals(m_CurrentlyEditingFile, StringComparison.OrdinalIgnoreCase))
                                    {
                                        //Remove all breakpoint carets up to the real line
                                        for (int line = fileLine.Item2; line < fileLine.Item3; line++)
                                        {
                                            if (currentCodeEditor.Document.BookmarkManager.IsMarked(line))
                                                currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, line));
                                        }

                                        //Set real breakpoint
                                        if (!currentCodeEditor.Document.BookmarkManager.IsMarked(fileLine.Item3))
                                            currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, fileLine.Item3));
                                    }
                                }
                            }

                            currentCodeEditor.Refresh();

                            // Set watchpoints
                             
                            foreach (uint address in m_Watchpoints)
                            {
                                m_Target.AddWatchpoint(address, address + 4);
                            }

                            //  Start
                            m_Target.Run();

                            //  profiling?
                            m_Profile = profilerEnabledMenuOptions.Checked;
                        };

                        BeginInvoke(action);
                    }

                    m_State = State.kRunning;

                    statusLabel.Text = "Running...";

                    action = () =>
                    {
                        currentCodeEditor.Document.ReadOnly = true;

                        // Reset the vdp status
                        m_VDPStatus.Reset();

                        //  Hide the build window
                        m_BuildLog.Hide();
                        
                        StartDebugging();
                    };

                    BeginInvoke(action);

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

        // bookmark manager needs
        //  ismarked
        //  togglemarkat
        //  clear

        private void toggleBreakpoint_Click(object sender, EventArgs e)
        {
            int lineNo = currentCodeEditor.ActiveTextAreaControl.Caret.Line;

            if (m_Breakpoints.Find(breakpoint => breakpoint.filename.Equals(m_CurrentlyEditingFile, StringComparison.OrdinalIgnoreCase) && breakpoint.line == lineNo) == null)
            {
                lineNo = SetBreakpoint(m_CurrentlyEditingFile, lineNo);

                if (!currentCodeEditor.Document.BookmarkManager.IsMarked(lineNo))
                    currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, lineNo));
            }
            else
            {
                lineNo = RemoveBreakpoint(m_CurrentlyEditingFile, lineNo);

                if (currentCodeEditor.Document.BookmarkManager.IsMarked(lineNo))
                    currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, lineNo));
            }

            currentCodeEditor.Refresh();
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
                string currentLineText = currentCodeEditor.Document.GetText(currentCodeEditor.Document.GetLineSegment(currentLine.Item3));
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
                int fileSizeLines = currentCodeEditor.Document.TotalNumberOfLines;

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
                m_MemoryView.Hide();
                memoryViewerToolStripMenuItem.Enabled = false;
                currentCodeEditor.Document.MarkerStrategy.Clear();

                statusLabel.Text = "Stopped";

                m_State = State.kStopped;
                currentCodeEditor.Document.ReadOnly = false;

                StopDebugging();

                //Restore original breakpoint lines
                for(int i = 0; i < m_Breakpoints.Count; i++)
                {
                    if(m_Breakpoints[i].originalLine > 0)
                    {
                        if (m_Breakpoints[i].filename.Equals(m_CurrentlyEditingFile, StringComparison.OrdinalIgnoreCase))
                        {
                            if (currentCodeEditor.Document.BookmarkManager.IsMarked(m_Breakpoints[i].line))
                                currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, m_Breakpoints[i].line));
                            if (!currentCodeEditor.Document.BookmarkManager.IsMarked(m_Breakpoints[i].originalLine))
                                currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, m_Breakpoints[i].originalLine));
                        }

                        m_Breakpoints[i].line = m_Breakpoints[i].originalLine;
                        m_Breakpoints[i].originalLine = 0;
                    }
                }

                currentCodeEditor.Refresh();
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

                currentCodeEditor.Encoding = Encoding.ASCII;
                currentCodeEditor.SaveFile(m_CurrentlyEditingFile);

                m_SourceWatcher.Path = Path.GetDirectoryName(m_CurrentlyEditingFile);
                m_SourceWatcher.Filter = Path.GetFileName(m_CurrentlyEditingFile);
                m_SourceWatcher.EnableRaisingEvents = true;
            }

            m_Modified = false;
            UpdateTitle();
        }

        public void GoTo(string filename, int lineNumber, bool isError = false)
        {
            if (m_CurrentlyEditingFile.ToLower() != filename.ToLower())
            {
                OpenFile(filename);
            }

            currentCodeEditor.ActiveTextAreaControl.Caret.Line = lineNumber;

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

            int offset = currentCodeEditor.Document.PositionToOffset(new TextLocation(0, lineNumberEditor));

            currentCodeEditor.Document.MarkerStrategy.Clear();

            if (lineNumberEditor < currentCodeEditor.Document.LineSegmentCollection.Count)
            {
                Marker marker = new Marker(offset, currentCodeEditor.Document.LineSegmentCollection[lineNumberEditor].Length, MarkerType.SolidBlock, Color.Yellow, Color.Black);//selection.Offset, selection.Length, MarkerType.SolidBlock, Color.DarkRed, Color.White);
                currentCodeEditor.Document.MarkerStrategy.AddMarker(marker);
                currentCodeEditor.ActiveTextAreaControl.Caret.Line = lineNumberEditor;
                currentCodeEditor.ActiveTextAreaControl.CenterViewOn(lineNumberEditor, -1);
            }
            else
            {
                currentCodeEditor.ActiveTextAreaControl.Caret.Line = lineNumberEditor;
                currentCodeEditor.ActiveTextAreaControl.CenterViewOn(0, -1);
            }

            currentCodeEditor.ActiveTextAreaControl.Caret.Column = 0;
            currentCodeEditor.ActiveTextAreaControl.Invalidate();
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
            var c = Color.DimGray;
            configForm.StartPosition = FormStartPosition.CenterParent;

            configForm.targetList.SelectedIndex = configForm.targetList.FindString(m_Config.TargetName);
            configForm.asm68kPath.Text = m_Config.Asm68kPath;
            configForm.AsPath.Text = m_Config.AsPath;
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
            configForm.ThemeCombobox.SelectedIndex = (int)m_Config.Theme;

            configForm.listIncludes.Items.Clear();
            if(m_Config.AssemblerIncludePaths != null)
            {
                foreach(string include in m_Config.AssemblerIncludePaths)
                    configForm.listIncludes.Items.Add(include);
            }            

            if (configForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_Config.TargetName = configForm.targetList.GetItemText(configForm.targetList.SelectedItem);
                m_Config.Asm68kPath = configForm.asm68kPath.Text;
                m_Config.AsPath = configForm.AsPath.Text;

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
                var oldTheme = m_Config.Theme;

                m_Config.Theme = (Theme)configForm.ThemeCombobox.SelectedIndex;

                if (oldTheme != m_Config.Theme)
                {
                    Invalidate();
                }

                // TODO: Change themes
                m_Config.Pal = configForm.modePAL.Checked;
                m_Config.MegaUSBPath = configForm.megaUSBPath.Text;

                m_Config.AssemblerIncludePaths = new string[configForm.listIncludes.Items.Count];
                configForm.listIncludes.Items.CopyTo(m_Config.AssemblerIncludePaths, 0);

                //m_Config.LightTheme = configForm.LightThemeSelection.Checked;
                m_Config.Save();

                Console.WriteLine(configForm.asm68kPath.Text);
                Console.WriteLine(configForm.AsPath.Text);

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
                m_Project = new Project();
                m_Project.Read(filename);
                m_PathToProject = Path.GetDirectoryName(filename);
                // Set project paths
                string path = $"{m_PathToProject}\\{m_Project.MainSourceFile}";
                m_ProjectFile = filename;
                //m_PathToProject = m_PathToProject; //Path.GetDirectoryName(filename);
                m_ProjectName = m_Project.Name; //Path.GetFileNameWithoutExtension(filename);
                m_FileToAssemble = path;
             
                // Open first file
                OpenFile(path);

                // Populate tree view
                PopulateFileView();
                treeProjectFiles.ExpandAll();

                // debugging
                // check if map/bin files exist already
                bool use_asm68k = m_Project.Assembler == Assembler.Asm68k ? true : false;
                string mainFileName = Path.GetFileNameWithoutExtension(m_Project.MainSourceFile);

                // need to delete some 
                var lstFile = $"{m_PathToProject}\\" + (use_asm68k ? $"{m_Project.Name}.list" : $"{mainFileName}.lst");
                string binaryFile = $"{m_PathToProject}\\" + (use_asm68k ? $"{m_Project.Name}.bin" : $"{mainFileName}.bin");
                string symbolFile = $"{m_PathToProject}\\" + (use_asm68k ? $"{m_Project.Name}.symb" : $"{mainFileName}.map");

                if (File.Exists(binaryFile) && File.Exists(symbolFile) && File.Exists(lstFile))
                {
                    // Need to initialize the symbol data before read
                    if (m_DebugSymbols == null)
                    {
                        if (m_Project.Assembler == Assembler.Asm68k)
                        {
                            m_DebugSymbols = new Asm68kSymbols();
                        }
                        else
                        {
                            m_DebugSymbols = new AsSymbols();
                        }
                    }
                    try
                    {
                        var filesToExclude = m_Project.FilesToExclude?.Select(e => $"{m_PathToProject}\\{e}".ToUpper().Replace("/", "\\"));
                        m_DebugSymbols.Read(symbolFile, filesToExclude?.ToArray());
                        m_AlreadyBuilt = true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Error reading debug data: {e.Message}");
                    }
                }

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

                        if (diskContents != currentCodeEditor.Document.TextContent)
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

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog pathSelect = new OpenFileDialog();

            pathSelect.Filter = "Mega Drive Project|*.mdproj";

            if (pathSelect.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ResetDocument();

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
            m_AlreadyBuilt = false;
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
                    using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
                    {
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

            Themes.UseImmersiveDarkMode(this.Handle, m_Config.Theme == Theme.Dark ? true : false);
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
                currentCodeEditor.Document.MarkerStrategy.RemoveMarker(marker);
            }

            m_SearchMarkers.Clear();
            m_SearchResults.Clear();
            m_SearchIndex = -1;

            if(requestUpdate)
            {
                currentCodeEditor.Refresh();
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
                    foreach (Match match in rx.Matches(currentCodeEditor.Document.TextContent))
                    {
                        TextLocation matchLocation = currentCodeEditor.Document.OffsetToPosition(match.Index);

                        Marker marker = new Marker(match.Index, match.Length, MarkerType.SolidBlock, Color.Orange, Color.Black);
                        currentCodeEditor.Document.MarkerStrategy.AddMarker(marker);

                        m_SearchMarkers.Add(marker);
                        m_SearchResults.Add(matchLocation);

                        if (firstFind)
                        {
                            m_SearchIndex++;
                            if (matchLocation.Line >= currentCodeEditor.ActiveTextAreaControl.Caret.Line)
                            {
                                currentCodeEditor.ActiveTextAreaControl.Caret.Position = matchLocation;
                                currentCodeEditor.ActiveTextAreaControl.CenterViewOn(matchLocation.Line, -1);
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

                currentCodeEditor.ActiveTextAreaControl.Caret.Position = m_SearchResults[m_SearchIndex];
                currentCodeEditor.ActiveTextAreaControl.CenterViewOn(m_SearchResults[m_SearchIndex].Line, -1);
            }
        }

        private void searchPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_SearchResults.Count > 0 && (m_SearchIndex - 1) >= 0)
            {
                m_SearchIndex--;

                currentCodeEditor.ActiveTextAreaControl.Caret.Position = m_SearchResults[m_SearchIndex];
                currentCodeEditor.ActiveTextAreaControl.CenterViewOn(m_SearchResults[m_SearchIndex].Line, -1);
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
                    currentCodeEditor.Document.TextContent = rx.Replace(currentCodeEditor.Document.TextContent, m_ReplaceString);
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
                    currentCodeEditor.ActiveTextAreaControl.Caret.Line = lineNumber - 1;
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
            if (currentCodeEditor != null)
            {
                currentCodeEditor.Document.DocumentChanged -= documentChanged;

                m_FileToAssemble = null;
                m_CurrentlyEditingFile = null;
                m_Modified = false;
                currentCodeEditor.Document.TextContent = null;
                m_SourceWatcher = null;
                currentCodeEditor.Refresh();
                currentCodeEditor.Document.UndoStack.ClearAll();
                currentCodeEditor.Document.BookmarkManager.Clear();
                UpdateTitle();
                treeProjectFiles.Nodes.Clear();
                m_ProjectFiles?.Clear();

                currentCodeEditor.Document.DocumentChanged += documentChanged;
            }

            foreach (TabPage page in DocumentTabs.TabPages)
            {
                this.DocumentTabs.TabPages.Remove(page);
            }

            DocumentTabs.TabPages.Clear();
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
            /*if (m_VDPViewer == null || m_VDPViewer.IsDisposed || m_VDPViewer.Disposing)
            {
                m_VDPViewer = new VdpPatternView();
            }*/
            m_VDPViewer.Show();
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

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectPropertiesView view = new ProjectPropertiesView(m_Project, m_PathToProject);
            view.PreBuildCommands = m_Project.PreBuildScript;
            view.PostBuildCommands = m_Project.PostBuildScript;
            view.PostBuildRunAlways = shouldAlwaysRunPostBuildCommand;


            if (view.ShowDialog() == DialogResult.OK)
            {
                m_Project.PreBuildScript = view.PreBuildCommands;
                m_Project.PostBuildScript = view.PostBuildCommands;
                shouldAlwaysRunPostBuildCommand = view.PostBuildRunAlways;
            }
            
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Project.Write(m_PathToProject);
        }

        private void memoryViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_State == State.kRunning)
            {
                byte[] buffer = new byte[0xFFFF];
                m_Target.ReadMemory(0xFFFF0000, 0xFFFF, buffer);
                m_MemoryView.SetRamMemory(buffer);
                m_MemoryView.Show();
            }
        }

        private void DocumentTabs_Selected(object sender, TabControlEventArgs e)
        {

            // Ignore document changed events
            bool watchingEvents = (m_SourceWatcher == null) || m_SourceWatcher.EnableRaisingEvents;
            m_SourceWatcher = null;

            if (e.TabPage != null)
            {

                TabPage page = e.TabPage;
                currentCodeEditor = (DigitalRune.Windows.TextEditor.TextEditorControl)page.Controls.Find(page.Text, true).First();
                // Reset undo state
                m_Modified = false;
                undoMenu.Enabled = false;

                // Load file
                // since tabpages use thi
                string file = page.Name;                               
                m_CurrentlyEditingFile = file;

                // Set title bar text
                this.Text = "MDStudio - " + m_CurrentlyEditingFile;

                // Populate known breakpoint markers
                foreach (Breakpoint breakpoint in m_Breakpoints)
                {
                    if (breakpoint.line != 0 && breakpoint.filename.ToLower() == m_CurrentlyEditingFile.ToLower())
                    {
                        if (!currentCodeEditor.Document.BookmarkManager.IsMarked(breakpoint.line))
                            currentCodeEditor.Document.BookmarkManager.ToggleMarkAt(new TextLocation(0, breakpoint.line));
                    }
                }

                // Resubscribe to document changed events
                currentCodeEditor.Document.DocumentChanged += documentChanged;
                m_SourceWatcher = new FileSystemWatcher();
                m_SourceWatcher.Path = Path.GetDirectoryName(m_CurrentlyEditingFile);
                m_SourceWatcher.Filter = Path.GetFileName(m_CurrentlyEditingFile);
                m_SourceWatcher.EnableRaisingEvents = watchingEvents;
                m_SourceWatcher.NotifyFilter = NotifyFilters.LastWrite;
                m_SourceWatcher.Changed += m_OnFileChanged;

                // Refresh
                currentCodeEditor.Refresh();

                m_SourceMode = SourceMode.Source;
            }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Source (*.asm;*.s;*.i)|*.ASM;*.S;*.I|All files (*.*)|*.*";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {                
                var files = m_Project.SourceFiles.ToList();
                // get project path length
                // need to add an extra \\ since source files shouldn't have them. 
                int count = $"{m_PathToProject}\\".Length;
                foreach (var file in ofd.FileNames)
                {
                    var filename = file.Remove(0, count);

                    if (!files.Contains(filename))
                    {
                        files.Add(filename);
                        m_ProjectFiles.Add(file);
                    }
                }

                m_Project.SourceFiles = files.ToArray();
                PopulateFileView();
            }
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to close your project?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (m_Modified)
                {
                    if (MessageBox.Show("Save changes?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Save();
                    }
                }

                using (NewProjectView newproject = new NewProjectView())
                {
                    if (newproject.ShowDialog() == DialogResult.OK)
                    {
                        ResetDocument();

                        OpenProject(newproject.ProjectFile);
                    }
                }
                    
                
            }
        }

        private void removeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveTab(this.DocumentTabs.SelectedIndex);
        }

        private void RemoveTab(int index)
        {
            this.DocumentTabs.TabPages.RemoveAt(index);
        }

        private void soundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_SoundOptions.Show();                            
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {            
            SetThemes(e.Graphics);
        }

        private void SetThemes(Graphics g)
        {
            var theme = m_Config.Theme;

            // only make changes if dark theme
            if (Themes.CurrentTheme != theme)
            {
                Themes.CurrentTheme = theme;
                Color bgColor = Themes.BackColor; //Color.FromArgb(255, 45, 45, 45);
                Color fgColor =Themes.ForeColor;
                // main form
                SetTheme(this, g, fgColor, bgColor);

                //this.treeProjectFiles.BackColor = this.BackColor;
                //this.treeProjectFiles.ForeColor = this.ForeColor;

                SetTheme(m_VDPRegs,g, fgColor, bgColor);
                SetTheme(m_VDPViewer,g, fgColor, bgColor);
                SetTheme(m_CRAMViewer,g, fgColor, bgColor);
                SetTheme(m_BuildLog,g, fgColor, bgColor);
                SetTheme(m_MemoryView,g, fgColor, bgColor);
                SetTheme(m_RegisterView,g, fgColor, bgColor);

                // also, adjust this too
                if (currentCodeEditor != null && currentCodeEditor.Document != null)
                {
                    // Set the syntax-highlighting for ASM68k
                    currentCodeEditor.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("ASM68k-Dark");
                }
            }
        }

        Type[] types = new Type[]
        {
            typeof(Control), typeof(MenuStrip), typeof(ToolStripMenuItem), typeof(ToolStrip), typeof(ScrollBar),typeof(TreeView),typeof(Form),
            typeof(SplitContainer), typeof(ScrollBar), typeof(SplitterPanel),typeof(ScrollBar),/*typeof(TabControl),typeof(TabPage),*/typeof(StatusStrip),
        };

        private void SetTheme(Form form,Graphics g, Color foreground, Color background)
        {
            form.BackColor = background;
            form.ForeColor = foreground;

            SetChildControlTheme(GetAllControls(form, types),g, foreground, background);

        }

        private void SetChildControlTheme(IEnumerable<Control> controls,Graphics g, Color foreground, Color background)
        {
            foreach (var control in controls)
            {
                control.BackColor = background;
                control.ForeColor = foreground;

                ControlPaint.DrawBorder(g, control.ClientRectangle,
                     background, 8, ButtonBorderStyle.Solid,
                     background, 8, ButtonBorderStyle.Solid,
                     background, 8, ButtonBorderStyle.Solid,
                    background, 8, ButtonBorderStyle.Solid);

                if (control.Controls.Count == 0)
                    continue;
                SetChildControlTheme(GetAllControls(control, types), g, foreground, background);
            }
        }

        public IEnumerable<Control> GetAllControls(Control control, Type[] types)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControls(ctrl, types))
                                      .Concat(controls).
                                      Where(c => types.Contains(c.GetType()));
        }

    }
}
