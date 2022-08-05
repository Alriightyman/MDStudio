using AvalonDock.Themes;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using MDStudioPlus.Debugging;
using MDStudioPlus.Editor;
using MDStudioPlus.Editor.BookMarks;
using MDStudioPlus.FileExplorer;
using MDStudioPlus.FileExplorer.Events;
using MDStudioPlus.Models;
using MDStudioPlus.Targets;
using MDStudioPlus.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MDStudioPlus.ViewModels
{
    internal class Workspace : ViewModelBase
    {
        #region Enumerations
        /// <summary>
        /// Editor states
        /// </summary>
        enum State
        {
            Stopped,
            Running,
            Debugging,
            Paused,
            Stopping,
        };

        /// <summary>
        /// 
        /// </summary>
        enum BreakMode
        {
            Breakpoint,
            StepOver,
            LogPoint
        };

        enum SourceMode
        {
            Source,
            Disassembly
        }
        #endregion

        #region Internal Classes
        /// <summary>
        /// Breakpoint only used here, in the workspace
        /// </summary>
        class Breakpoint
        {
            public string Filename { get; set; }
            public int Line { get; set; }
            public int OriginalLine { get; set; }
            public uint Address { get; set; }
            public Breakpoint(string _file, int _line) { Filename = _file; Line = _line; Address = 0; OriginalLine = 0; }
            public Breakpoint(string _file, int _line, uint _address) { Filename = _file; Line = _line; Address = _address; OriginalLine = 0; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Workspace instance for global access
        /// </summary>
        public static Workspace Instance => instance;

        /// <summary>
        /// Regions
        /// </summary>
        public static readonly ReadOnlyCollection<Tuple<char, string>> Regions = new ReadOnlyCollection<Tuple<char, string>>(new[]
        {
            new Tuple<char, string>( 'J', "Japan" ),
            new Tuple<char, string>( 'U', "USA" ),
            new Tuple<char, string>( 'E', "Europe" )
        });
        
        private static Workspace instance = new Workspace();
        
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

        private IntPtr handle;

        private ToolViewModel[] tools;
        private ObservableCollection<FileViewModel> files = new ObservableCollection<FileViewModel>();
        private ReadOnlyObservableCollection<FileViewModel> readonyFiles;
        private CodeEditor codeEditor;
        private FileViewModel activeDocument;
        private ErrorViewModel errors;
        private ExplorerViewModel explorer;
        private OutputViewModel output;
        private ConfigViewModel configViewModel;
        private RegistersViewModel registersViewModel;
        private MemoryViewModel memoryViewModel;
        private SolidColorBrush statusBackgroundColor = (SolidColorBrush)Application.Current.Resources["StatusBarBackground"];
        
        // commands
        private RelayCommand openProjectSolutionCommand;
        private RelayCommand openFileCommand;
        private RelayCommand newSolutionCommand;
        private RelayCommand newProjectCommand;
        private RelayCommand newFileCommand;
        private RelayCommand saveFileCommand;
        private RelayCommand saveAllCommand;
        private RelayCommand closeSolutionCommand;
        private RelayCommand openProjectPropertiesWindowCommand;
        private RelayCommand buildSolutionCommand;
        private RelayCommand runEmulatorCommand;
        private RelayCommand stopEmulatorCommand;
        private RelayCommand configurationCommand;
        private RelayCommand aboutCommand;
        private RelayCommand<BookmarkEventArgs> onBreakpointAddedCommand;
        private RelayCommand onBreakpointRemovedCommand;
        private RelayCommand<BookmarkEventArgs> onBreakPointAfterAddedCommand;
        private RelayCommand<BookmarkEventArgs> onBreakpointBeforeRemovedCommand;
        private RelayCommand restartEmulatorCommand;
        private RelayCommand breakAllCommand;
        private RelayCommand stepIntoCommand;
        private RelayCommand stepOverCommand;

        // themes
        private Tuple<string, Theme> selectedTheme;

        private State state;
        private FileSystemWatcher sourceWatcher;
        private FileSystemEventHandler onFileChanged;
        private object watcherCritSec = new object();
        private bool isDebugging = false;
        private ISymbols debugSymbols;

        // emulator
        private Target target;
        private readonly DispatcherTimer timer = new DispatcherTimer();



        // project/solution
        private Solution solution;
        private string solutionName = "No Project Opened";
        private bool isSolutionLoaded = false;
        private Project selectedProject = null;

        // building
        private bool alreadyBuilt = false;
        private bool isBuilding = false;
        private List<Error> ErrorMarkers = new List<Error>();

        private string status = "Ready";

        private BreakMode breakMode = BreakMode.Breakpoint;

        private SourceMode sourceMode = SourceMode.Source;
        private Breakpoint stepOverBreakpoint;

        private List<Breakpoint> breakpoints;
        private List<uint> watchpoints;


        #endregion

        #region Events
        public event EventHandler ActiveDocumentChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public Workspace()
        {
            ConfigViewModel.OnThemeChanged += ConfigViewModel_OnThemeChanged;
            ConfigViewModel.Initialize();

            breakpoints = new List<Breakpoint>();

            //try
            {
                //target = TargetFactory.Create();
                target = new TargetDGen();
                // updat config
            }

            timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;

            onFileChanged = new FileSystemEventHandler(OnSourceChanged);

            StopDebugging();
        }

        private void ConfigViewModel_OnThemeChanged(object sender, SelectThemeEventArgs e)
        {
            SelectedTheme = Themes.FirstOrDefault(t => t.Item1 == e.Name);
            //SwitchExtendedTheme();
        }

        #endregion

        #region Properties        
        private string projectPropertiesHeader = "Properties";
        public string ProjectPropertiesHeader
        {
            get => projectPropertiesHeader;
            set
            {
                projectPropertiesHeader = $"{(value)} Properties";
                RaisePropertyChanged(nameof(ProjectPropertiesHeader));
            }
        }

        /// <summary>
        /// Set when the application is building
        /// </summary>
        public bool IsBuilding
        {
            get => isBuilding;
            set
            {
                isBuilding = value;
                RaisePropertyChanged(nameof(IsBuilding));
            }
        }

        private bool isBreakpointHit;
        public bool IsBreakPointHit
        {
            get => isBreakpointHit;
           /* {
                if (state == State.Paused)
                {
                    return true;
                }

                return false;                
            }*/
            set
            {
                isBreakpointHit = value;
                RaisePropertyChanged(nameof(IsBreakPointHit));
                Activated = value;
                RaisePropertyChanged(nameof(Activated));
            }
        }

        public bool Activated { get; set; } = true;

        /// <summary>
        /// Set when a Solution has been loaded
        /// </summary>
        public bool IsSolutionLoaded
        {
            get => isSolutionLoaded;
            set
            {
                isSolutionLoaded = value;
                RaisePropertyChanged(nameof(IsSolutionLoaded));
            }
        }

        /// <summary>
        /// Set while the emulator is running (or debugging)
        /// </summary>
        public bool IsDebugging
        {
            get => isDebugging;
            set
            {
                isDebugging = value;
                RaisePropertyChanged(nameof(isDebugging));

                if(isDebugging)
                {
                    StatusBackgroundColor = (SolidColorBrush)Application.Current.Resources["StatusBarBackgroundDebugging"];
                    Explorer.IsVisible = false;
                    Registers.IsVisible = true;
                    Registers.IsSelected = true;
                    Memory.IsVisible = true;
                    Memory.IsSelected = true;
                    Errors.IsVisible = false;
                }
                else
                {
                    StatusBackgroundColor = (SolidColorBrush)Application.Current.Resources["StatusBarBackground"];
                    Explorer.IsVisible = true;
                    Registers.IsVisible = false;
                    Memory.IsVisible = false;
                    Errors.IsVisible = true;                    
                }

            }
        }

        /// <summary>
        /// Bound to the status bar - shows when building and other information
        /// </summary>
        public string Status
        {
            get => status;
            set
            {
                status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Shows the solution name on the top
        /// </summary>
        public string SolutionName
        {
            get
            {
                return solutionName;
            }
            set
            {
                solutionName = value;
                RaisePropertyChanged(nameof(solutionName));
            }
        }

        /// <summary>
        /// Gets the loaded solution
        /// </summary>
        public Solution Solution => solution;

        /// <summary>
        /// Bound to the Docking manager
        /// </summary>
        public ReadOnlyObservableCollection<FileViewModel> Files
        {
            get
            {
                if (readonyFiles == null)
                    readonyFiles = new ReadOnlyObservableCollection<FileViewModel>(files);

                return readonyFiles;
            }
        }
        
        /// <summary>
        /// View model for all configuration options
        /// </summary>
        public ConfigViewModel ConfigViewModel
        {
            get
            {
                if (configViewModel == null)
                {
                    configViewModel = new ConfigViewModel();
                }

                return configViewModel;
            }
        }

        /// <summary>
        /// View Model shows files, folders, projects and solutions in a dockable window
        /// </summary>
        public ExplorerViewModel Explorer
        {
            get
            {
                if (explorer == null)
                {
                    explorer = new ExplorerViewModel();
                    explorer.OnSelectedItemChanged -= Explorer_OnSelectedItemChanged;
                    explorer.OnSelectedItemChanged += Explorer_OnSelectedItemChanged;
                }

                return explorer;
            }
        }

        public RegistersViewModel Registers
        {
            get
            {
                if(registersViewModel == null)
                {
                    registersViewModel = new RegistersViewModel();
                }

                return registersViewModel;
            }
        }

        public MemoryViewModel Memory
        {
            get
            {
                if (memoryViewModel == null)
                {
                    memoryViewModel = new MemoryViewModel();
                }

                return memoryViewModel;
            }
        }

        /// <summary>
        /// Shows errors in the error window
        /// </summary>
        public ErrorViewModel Errors
        {
            get
            {
                if (errors == null)
                    errors = new ErrorViewModel();

                return errors;
            }
        }

        /// <summary>
        /// Shows output when building
        /// </summary>
        public OutputViewModel Output
        {
            get
            {
                if (output == null)
                    output = new OutputViewModel();

                return output;
            }
        }

        /// <summary>
        /// Various dockable windows
        /// </summary>
        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                if (tools == null)
                    // TODO: Add debugging windows here - CRAM Viewer, Register View, etc.
                    tools = new ToolViewModel[] { Explorer, Errors, Output, Registers, Memory };
                return tools;
            }
        }

        /// <summary>
        /// Current document being editted
        /// </summary>
        public FileViewModel ActiveDocument
        {
            get => activeDocument;
            set
            {
                if (activeDocument != value)
                {
                    // unhook events
                    if (activeDocument != null) activeDocument.OnFileSelected -= ActiveDocument_OnFileSelected;
                    
                    activeDocument = value;
                    
                    // only handle if not null
                    if (activeDocument != null)
                    {
                        activeDocument.OnFileSelected += ActiveDocument_OnFileSelected;
                        if (ActiveDocumentChanged != null)
                            ActiveDocumentChanged(this, EventArgs.Empty);
                    }
                    RaisePropertyChanged(nameof(ActiveDocument));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Tuple<string, Theme>> Themes { get; set; } = new List<Tuple<string, Theme>>
        {
            new Tuple<string, Theme>("Dark Theme",new Vs2013DarkTheme()),
            new Tuple<string, Theme>("Light Theme",new Vs2013LightTheme()),
            new Tuple<string, Theme>("Blue Theme",new Vs2013BlueTheme())
        };


        public Tuple<string, Theme> SelectedTheme
        {
            get { return selectedTheme; }
            set
            {
                selectedTheme = value;
                UpdateSyntaxHighlighting();
                RaisePropertyChanged(nameof(SelectedTheme));
            }
        }
       
        public SolidColorBrush StatusBackgroundColor
        {
            get => statusBackgroundColor;
            set
            {
                statusBackgroundColor = value;
                RaisePropertyChanged(nameof (StatusBackgroundColor));
            }
    
        }

        #endregion

        #region Commands
        public ICommand OpenProjectSolutionCommand
        {
            get
            {
                if (openProjectSolutionCommand == null)
                {
                    openProjectSolutionCommand = new RelayCommand((p) => OnOpenProjectSolution(), (p) => CanOpen());
                }

                return openProjectSolutionCommand;
            }
        }
        public ICommand OpenFileCommand
        {
            get
            {
                if (openFileCommand == null)
                {
                    openFileCommand = new RelayCommand((p) => OnOpenFile(), (p) => CanOpen());
                }

                return openFileCommand;
            }
        }

        public ICommand NewSolutionCommand
        {
            get
            {
                if (newSolutionCommand == null)
                {
                    newSolutionCommand = new RelayCommand((p) => OnNewSolution(), (p) => CanNew());
                }

                return newSolutionCommand;
            }
        }
        public ICommand NewProjectCommand
        {
            get
            {
                if (newProjectCommand == null)
                {
                    newProjectCommand = new RelayCommand((p) => OnNewProject(), (p) => CanNew());
                }

                return newProjectCommand;
            }
        }
        public ICommand NewFileCommand
        {
            get
            {
                if (newFileCommand == null)
                {
                    newFileCommand = new RelayCommand((p) => OnNewFile(), (p) => CanNew());
                }

                return newFileCommand;
            }
        }

        public ICommand BuildSolutionCommand
        {
            get
            {
                if (buildSolutionCommand == null)
                {
                    buildSolutionCommand = new RelayCommand((p) => OnBuild(), (p) => CanBuild());
                }

                return buildSolutionCommand;
            }
        }

        public ICommand RunEmulatorCommand
        {
            get
            {
                if (runEmulatorCommand == null)
                {
                    runEmulatorCommand = new RelayCommand((p) => OnRunEmulator(), (p) => CanRunEmulator());
                }

                return runEmulatorCommand;
            }
        }

        public ICommand StopEmulatorCommand
        {
            get
            {
                if (stopEmulatorCommand == null)
                {
                    stopEmulatorCommand = new RelayCommand((p) => OnStopEmulator(), (p) => CanStopEmulator());
                }

                return stopEmulatorCommand;
            }
        }

        public ICommand RestartEmulatorCommand
        {
            get
            {
                if (restartEmulatorCommand == null)
                {
                    restartEmulatorCommand = new RelayCommand((p) => OnRestartEmulator(), (p) => true);
                }

                return restartEmulatorCommand;
            }
        }

        public ICommand BreakAllCommand
        {
            get
            {
                if(breakAllCommand == null)
                {
                    breakAllCommand = new RelayCommand((p) => OnBreakAll());
                }

                return breakAllCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (saveFileCommand == null)
                {
                    saveFileCommand = new RelayCommand((p) => OnSave(), (p) => ActiveDocument != null);
                }

                return saveFileCommand;
            }
        }

        public ICommand SaveAllCommand
        {
            get
            {
                if (saveAllCommand == null)
                {
                    saveAllCommand = new RelayCommand((p) => OnSaveAll(), (p) => files.Count > 0);
                }

                return saveAllCommand;
            }
        }

        public ICommand CloseSolutionCommand
        {
            get
            {
                if (closeSolutionCommand == null)
                {
                    closeSolutionCommand = new RelayCommand((p) => OnSolutionClose());
                }

                return closeSolutionCommand;
            }
        }

        public ICommand OpenProjectPropertiesWindowCommand
        {
            get
            {
                if(openProjectPropertiesWindowCommand == null)
                {
                    openProjectPropertiesWindowCommand = new RelayCommand((p) => OnOpenProjectProperties(), (p) => IsSolutionLoaded);
                }
                return openProjectPropertiesWindowCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if(aboutCommand == null)
                {
                    aboutCommand = new RelayCommand((p) => OnAboutWindowOpen(), (p) => true);
                }

                return aboutCommand;
            }
        }

        public ICommand ConfigurationCommand
        {
            get
            {
                if (configurationCommand == null)
                {
                    configurationCommand = new RelayCommand((p) => OnConfigurationOpen(), (p) => true);
                }

                return configurationCommand;
            }
        }


        public ICommand OnBreakPointAddedCommand
        {
            get
            {
                if (onBreakpointAddedCommand == null)
                {
                    onBreakpointAddedCommand = new RelayCommand<BookmarkEventArgs>((p) => OnBreakpointAdded(p));
                }

                return onBreakpointAddedCommand;
            }
        }
        public ICommand OnBreakPointAfterAddedCommand
        {
            get
            {
                if (onBreakPointAfterAddedCommand == null)
                {
                    onBreakPointAfterAddedCommand = new RelayCommand<BookmarkEventArgs>((p) => OnBreakPointBeforeAdded(p),(p) => true);
                }

                return onBreakPointAfterAddedCommand;
            }
        }

        public ICommand OnBreakPointBeforeRemovedCommand
        {
            get
            {
                if (onBreakpointBeforeRemovedCommand == null)
                {
                    onBreakpointBeforeRemovedCommand = new RelayCommand<BookmarkEventArgs>((p) => OnBreakpointBeforeRemoved(p));
                }

                return onBreakpointBeforeRemovedCommand;
            }
        }

        public ICommand OnBreakPointRemovedCommand
        {
            get
            {
                if (onBreakpointRemovedCommand == null)
                {
                    onBreakpointRemovedCommand = new RelayCommand((p) => OnBreakpointRemoved(p));
                }

                return onBreakpointRemovedCommand;
            }
        }

        public ICommand StepIntoCommand
        {
            get
            {
                if(stepIntoCommand == null)
                {
                    stepIntoCommand = new RelayCommand((p) => StepInto());
                }

                return stepIntoCommand;
            }
        }

        public ICommand StepOverCommand
        {
            get
            {
                if (stepOverCommand == null)
                {
                    stepOverCommand = new RelayCommand((p) => StepOver());
                }

                return stepOverCommand;
            }
        }

        private RelayCommand<MouseEventArgs> mouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get
            {
                if(mouseMoveCommand == null)
                {
                    mouseMoveCommand = new RelayCommand<MouseEventArgs>((p) => OnMouseMove(p));
                }
                return mouseMoveCommand;
            }
        }

        #endregion

        #region Events

        private void OnMouseMove(MouseEventArgs e)
        {
            // don't update this if the game is running..
            if (IsBreakPointHit)
            {
                ActiveDocument?.SetWordAtMousePosition(e);
            }
        }

        private void Explorer_OnSelectedItemChanged(object sender, SelectedItemEventArgs e)
        {
            if (e.SelectedItem is FileExplorer.FileItemViewModel || e.SelectedItem is FileExplorer.ProjectItemViewModel)
            {
                var fileViewModel = Open(e.SelectedItem.Path);
                
                if (e.SelectedItem is IProjectItemChild ipic)
                {
                    fileViewModel.Project = ipic.Project;
                }

                ProjectPropertiesHeader = fileViewModel.Project?.Name ?? "";                

                fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((target != null && target.IsHalted())
                && state == State.Running)
            {
                if (breakMode == BreakMode.LogPoint && watchpoints.Count > 0)
                {
                    //Log point, fetch new value, log and continue
                    uint value = target.ReadLong(watchpoints[0]);
                    String log = String.Format("LOGPOINT - Address 0x{0:x} = 0x{1:x}", watchpoints[0], value);
                    Debug.WriteLine(log);
                    target.Resume();
                }
                else
                {
                    //Breakpoint hit, go to address
                    Application.Current.Dispatcher.Invoke(() => IsBreakPointHit = true);
                    uint currentPC = target.GetPC();
                    GoTo(currentPC);

                    // TODO: update registers and call stack here
                    UpdateRegisterView(currentPC);

                    if (breakMode == BreakMode.StepOver)
                    {
                        //If hit desired step over address
                        if (currentPC == stepOverBreakpoint.Address)
                        {
                            //Clear step over breakpoint, if we don't have a user breakpoint here
                            if (breakpoints.IndexOf(stepOverBreakpoint) == -1)
                            {
                                target.RemoveBreakpoint(stepOverBreakpoint.Address);                                
                            }

                            //Return to breakpoint mode
                            stepOverBreakpoint = null;

                            breakMode = BreakMode.Breakpoint;
                        }
                        else
                        {
                            Console.WriteLine("Step-over hit unexpected breakpoint at " + currentPC);
                        }
                    }

                    //In breakpoint state
                    state = State.Debugging;
                }
            }
            else if(target != null && state == State.Running)
            {
                // TODO update debug windows
                UpdateMemoryView();
            }
        }
        #endregion

        #region Public Methods
        public void WindowLoaded(string[] cmdlineArgs)
        {
            var window = Application.Current.MainWindow;
            handle = new WindowInteropHelper(window).Handle;

            if (cmdlineArgs.Length > 1)
            {
                OpenSolution(cmdlineArgs[1]);
            }
            else if (ConfigViewModel.Config.AutoOpenLastProject)
            {
                //Open last project
                OpenSolution(ConfigViewModel.Config.LastProject);
            }            
        }

        public void UpdateDocument()
        {
            // add the error markers if any
            foreach (var error in ErrorMarkers)
            {
                if (ActiveDocument.FileName.Trim('*') == error.Filename)
                {
                    ActiveDocument.AddMarker(Int32.Parse(error.LineNumber), TextMarkerTypes.SquigglyUnderline, Colors.Red);
                }
            }

        }

        public void UpdateSyntaxHighlighting()
        {           
            if (activeDocument != null) activeDocument.SyntaxHighlightName = GetCurrentHightlighting();
        }

        public void AddErrorMarkerToDocument(Project project, Error error)
        {
            var file = project.AllFiles().Where(f => f == error.Filename).FirstOrDefault();

            var fileViewModel = files.FirstOrDefault(fm => $"{fm.FileName}".ToLower() == file.ToLower());
            if (fileViewModel == null)
            {
                fileViewModel = new FileViewModel($"{project.ProjectPath}\\{file}", project);
                fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
            }

            fileViewModel.AddMarker(Int32.Parse(error.LineNumber), TextMarkerTypes.SquigglyUnderline, Colors.Red);
            ErrorMarkers.Add(error);            
        }

        public void GoTo(string filename, int lineNumber, bool isError = false)
        {
            string fullFilename = Solution.GetFullPath(filename);

            if(ActiveDocument?.FileName != fullFilename)
            {
                // open with the active documents filename instead
                // otherwise, you get UPPER case filename and can 
                // open the same file but in lower case..
                Open(ActiveDocument?.FileName ?? fullFilename);
            }

            // set priority lower priority level so we can guarenttee 
            // the DocumentChanged event has fired and we can access
            // the CodeEditor control 
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                CodeEditor codeEd = ActiveDocument.Editor;
                if (codeEd != null)
                {
                    var line = ActiveDocument.Document.GetLineByNumber(lineNumber);
                    codeEd.CaretOffset = line.Offset;
                    codeEd.TextArea.Caret.BringCaretToView();
                }
            }),DispatcherPriority.Render);
        }

        public void GoTo(uint address)
        {
            DebugInfo currentLine = debugSymbols.GetFileLine(address);

            string filename = currentLine.Filename;
            int lineNumberSymbols = currentLine.LineFrom;
            int lineNumberEditor = currentLine.LineTo;

            if (lineNumberSymbols >= 0 && filename.Length > 0)
            {
                filename = filename.ToLower();
                //Load file
                if (ActiveDocument?.FilePath.ToLower() != filename)
                {
                    Open(filename);
                }
            }
            else
            {
                // TODO: Display Disassembly

            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ActiveDocument.RemoveAllMarkers();

                if (lineNumberEditor < ActiveDocument.Document.LineCount)
                {

                    ActiveDocument.AddMarker(lineNumberEditor, TextMarkerTypes.RectangleHighlight, Colors.Yellow);

                    CodeEditor codeEd = ActiveDocument.Editor;
                    if (codeEd != null)
                    {
                        var line = ActiveDocument.Document.GetLineByNumber(lineNumberEditor);
                        codeEd.CaretOffset = line.Offset;
                        codeEd.TextArea.Caret.BringCaretToView();
                    }
                }
                else
                {
                    CodeEditor codeEd = ActiveDocument.Editor;
                    if (codeEd != null)
                    {
                        var line = ActiveDocument.Document.GetLineByNumber(lineNumberEditor);
                        codeEd.CaretOffset = line.Offset;
                        codeEd.TextArea.Caret.BringCaretToView();
                    }
                }

                activeDocument.Refresh();

            }), DispatcherPriority.Render);
        }

        private void ActiveDocument_OnFileSelected(object sender, FileViewModelSelectedEventArgs e)
        {
            // Ignore document changed events
            bool watchingEvents = (sourceWatcher == null) || sourceWatcher.EnableRaisingEvents;

            if (sourceWatcher != null)
            {
                sourceWatcher.Changed -= onFileChanged;                
                sourceWatcher = null;
            }

            // TODO: File watcher to watch all directories instead of individual files?
            sourceWatcher = new FileSystemWatcher();
            sourceWatcher.Path = Path.GetDirectoryName(e.SelectedFile.FilePath);
            sourceWatcher.Filter = Path.GetFileName(e.SelectedFile.FilePath);
            sourceWatcher.EnableRaisingEvents = watchingEvents;
            sourceWatcher.NotifyFilter = NotifyFilters.LastWrite;
            sourceWatcher.Changed += onFileChanged;
        }

        #endregion

        #region Private Methods

        private void UpdateRegisterView(uint pc)
        {
            //Get regs
            uint[] dregs = new uint[8];
            for (int i = 0; i < 8; i++)
            {
                dregs[i] = target.GetDReg(i);
            }

            uint[] aregs = new uint[8];
            for (int i = 0; i < 8; i++)
            {
                aregs[i] = target.GetAReg(i);
            }

            uint sr = target.GetSR();

            // add 68k register values
            Application.Current.Dispatcher.Invoke(() => Registers.UpdateRegisterValues(dregs, aregs, sr, pc));
            // update z80 register values
            Application.Current.Dispatcher.Invoke(() => Registers.UpdateZ80Registers(target.GetZ80Reg(Z80Regs.FA),
                                                                                     target.GetZ80Reg(Z80Regs.CB),
                                                                                     target.GetZ80Reg(Z80Regs.ED),
                                                                                     target.GetZ80Reg(Z80Regs.LH),
                                                                                     target.GetZ80Reg(Z80Regs.FA_ALT),
                                                                                     target.GetZ80Reg(Z80Regs.CB_ALT),
                                                                                     target.GetZ80Reg(Z80Regs.ED_ALT),
                                                                                     target.GetZ80Reg(Z80Regs.LH_ALT),
                                                                                     target.GetZ80Reg(Z80Regs.IX),
                                                                                     target.GetZ80Reg(Z80Regs.IY),
                                                                                     target.GetZ80Reg(Z80Regs.SP),
                                                                                     target.GetZ80Reg(Z80Regs.PC)));
        }

        private void UpdateMemoryView()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {               
                byte[] memBuffer = new byte[0x10000];
                target.ReadMemory(0xFFFF0000, 0x10000, memBuffer);
                Memory.UpdateMemory(memBuffer);
            });
        }

        internal void Close(FileViewModel fileToClose)
        {
            if (fileToClose.IsDirty)
            {
                var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                    return;
                if (res == MessageBoxResult.Yes)
                {
                    Save(fileToClose);
                }
            }

            ResetDocument();

            files.Remove(fileToClose);
        }

        internal void OnSolutionClose()
        {
            var allFiles = files.ToList();
            foreach(var file in allFiles)
            {
                Close(file);
            }
            files.Clear();
            solution = null;
            IsSolutionLoaded = false;
            SolutionName = "No Project Loaded";            
            Explorer.Solution = null;
            output.BuildOutput = "";
            Errors.Clear();
        }

        internal void OnSave()
        {
            Save(ActiveDocument);
        }

        internal void OnSaveAll()
        {
            SaveAll();
        }

        internal void SaveAll()
        {
            foreach(var file in files)
            {
                Action action = () => Save(file);
                System.Windows.Application.Current.Dispatcher.Invoke((action));
            }
        }

        internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
        {
            if (fileToSave != null)
            {
                string newTitle = string.Empty;
                if (fileToSave?.FilePath == null || saveAsFlag)
                {
                    var dlg = new SaveFileDialog();

                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        fileToSave.FilePath = dlg.FileName;
                        newTitle = dlg.SafeFileName;
                    }
                }

                if (fileToSave.FilePath != null)
                {
                    if (sourceWatcher != null) sourceWatcher.EnableRaisingEvents = false;

                    File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);

                    if (sourceWatcher != null)
                    {
                        sourceWatcher.Path = Path.GetDirectoryName(fileToSave.FilePath);
                        sourceWatcher.Filter = Path.GetFileName(fileToSave.FilePath);
                        sourceWatcher.EnableRaisingEvents = true;
                    }

                    ActiveDocument.IsDirty = false;

                    if (string.IsNullOrEmpty(newTitle)) return;

                    ActiveDocument.Title = newTitle;
                }
            }
        }

        internal FileViewModel Open(string filepath)
        {
            FileViewModel fileViewModel = null;
            filepath = filepath.Replace("*",String.Empty);
            if (File.Exists(filepath))
            {
                Project project = null;
                fileViewModel = files.FirstOrDefault(fm => fm.FilePath.ToLower() == filepath.ToLower());
                if (fileViewModel == null)
                {
                    project = solution.Projects.FirstOrDefault(p => p.AllFiles().Any(f => f == filepath.ToLower()));
                    fileViewModel = new FileViewModel(filepath, project);
                    fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
                    files.Add(fileViewModel);
                }

                ActiveDocument = fileViewModel;

                // Ignore document changed events
                bool watchingEvents = (sourceWatcher == null) || sourceWatcher.EnableRaisingEvents;
                sourceWatcher.Changed -= onFileChanged;
                sourceWatcher = null;

                sourceWatcher = new FileSystemWatcher();
                sourceWatcher.Path = Path.GetDirectoryName(ActiveDocument.FilePath);
                sourceWatcher.Filter = Path.GetFileName(ActiveDocument.FilePath);
                sourceWatcher.EnableRaisingEvents = watchingEvents;
                sourceWatcher.NotifyFilter = NotifyFilters.LastWrite;
                sourceWatcher.Changed += onFileChanged;

                sourceMode = SourceMode.Source;

                // These need to run after binding events in order to access the "Code Editor" in ActiveDocument
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (Breakpoint breakpoint in breakpoints)
                    {
                        if (breakpoint.Line != 0 && breakpoint.Filename.ToLower() == ActiveDocument.FilePath.ToLower())
                        {
                            if (!ActiveDocument.IsBreakpointSet(breakpoint.Line))
                            {
                                ActiveDocument?.ToggleBreakpoint(breakpoint.Line);
                            }
                        }
                    }
                }), DispatcherPriority.Render);
                
            }
            return fileViewModel;
        }

        #region Debugging
        // TODO: Add debugging related stuff here
        private void StartDebugging()
        {
            //throw new NotImplementedException();
            //stepIntoMenu.Enabled = true;
            //stepOverMenu.Enabled = true;
            //stopToolStripMenuItem.Enabled = true;
            //breakMenu.Enabled = true;
        }

        // TODO: Turn off debugging related stuff here
        private void StopDebugging()
        {
            //throw new NotImplementedException();
            //stepIntoMenu.Enabled = false;
            //stepOverMenu.Enabled = false;
            //stopToolStripMenuItem.Enabled = false;
            //breakMenu.Enabled = false;
            //m_Watchpoints.Clear();
        }

        private void SetTargetBreakpoint(uint address)
        {
            if (target != null)
            {
                target.AddBreakpoint(address);
            }
        }

        private void RemoveTargetBreakpoint(uint address)
        {
            if (target != null)
            {
                target.RemoveBreakpoint(address);
            }
        }

        public int SetBreakpoint(string filename, int line)
        {
            if (state != State.Stopped && debugSymbols != null)
            {
                //Symbols loaded, add "online" breakpoint by address
                uint address = debugSymbols.GetAddress(filename, line);
                if (address >= 0)
                {
                    //if (m_BreakpointView != null)
                    //{
                    //    //TODO: Breakpoint view should be by file/line, not address
                    //    m_BreakpointView.SetBreakpoint(address);
                    //}

                    SetTargetBreakpoint(address);
                    breakpoints.Add(new Breakpoint(filename, line, address));

                    //Actual line might be different from requested line, look it up
                    return debugSymbols.GetFileLine(filename, address).LineTo;
                }

                return line;
            }
            else
            {
                //No symbols yet, add "offline" breakpoint by file/line
                breakpoints.Add(new Breakpoint(filename, line));
                return line;
            }
        }

        public int RemoveBreakpoint(string filename, int line)
        {
            if (state != State.Stopped && debugSymbols != null)
            {
                uint address = debugSymbols.GetAddress(filename, line);
                if (address >= 0) 
                {
                    //if (breakpointView != null)
                    //{
                    //    //TODO: Breakpoint view should be by file/line, not address
                    //    breakpointView.RemoveBreakpoint(address);
                    //}

                    RemoveTargetBreakpoint(address);

                    /*breakpoints.Remove(breakpoints.Find(breakpoint => 
                                                        breakpoint.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase) && 
                                                        breakpoint.Line == line));*/

                    //Actual line might be different from requested line, look it up
                    return debugSymbols.GetFileLine(filename, address).LineTo;
                }

                return line;
            }
            else
            {
                breakpoints.Remove(breakpoints.Find(breakpoint => 
                                                    breakpoint.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase) && 
                                                    breakpoint.Line == line));
                return line;
            }
        }

        public void ClearBreakpoints()
        {
            //if (breakpointView != null)
            //{
            //    breakpointView.ClearBreakpoints();
            //}

            if (target != null)
            {
                target.RemoveAllBreakpoints();
            }

            breakpoints.Clear();
        }

        private void StepInto()
        {
            //Step to next instruction (blocking)
            target.Step();

            //Go to address
            uint currentPC = target.GetPC();
            GoTo(currentPC);

            //Re-evaluate on next timer tick
            state = State.Running;
        }

        private void StepOver()
        {
            //TODO: Breakpoints in disassembly mode
            if (sourceMode == SourceMode.Disassembly)
            {
                StepInto();
            }
            else
            {
                //Get current address
                uint currentPC = target.GetPC();
                uint nextPC = currentPC;

                //Get current file/line
                DebugInfo currentLine = debugSymbols.GetFileLine((uint)currentPC);
                int nextLine = currentLine.LineTo;

                //Determine if current instruction should be stepped into
                //TODO: Add instruction peek to DGen, determine by opcode

                if (ActiveDocument == null)
                {
                    Open(currentLine.Filename);
                }

                DocumentLine line = null; 
                string currentLineText = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    line = ActiveDocument.Document.GetLineByNumber(currentLine.LineTo);
                    currentLineText = ActiveDocument.Document.GetText(line);
                });

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
                        StepInto();
                        return;
                    }
                }

                //Get total num lines
                //TODO: Verify current filename in editor matches emulator?
                int fileSizeLines = ActiveDocument.Document.LineCount;

                //Ignore lines with same address as current
                while (currentPC == nextPC)
                {
                    //Get next line
                    nextLine++;

                    //If next line is in another file, step into instead
                    if (nextLine > fileSizeLines)
                    {
                        StepInto();
                        return;
                    }

                    //Get address of next line
                    nextPC = debugSymbols.GetAddress(currentLine.Filename, nextLine);
                }

                //Set breakpoint at next address
                target.AddBreakpoint(nextPC);

                //Set StepOver mode
                breakMode = BreakMode.StepOver;
                stepOverBreakpoint = new Breakpoint(currentLine.Filename, nextLine, nextPC);
                
                //Run to StepOver breakpoint
                target.Resume();
                state = State.Running;
            }
        }
        #endregion

        #region File Watching
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
            if (sourceMode == SourceMode.Source)
            {
                if (System.Threading.Monitor.TryEnter(watcherCritSec))
                {
                    // Disable further checks
                    sourceWatcher.EnableRaisingEvents = false;

                    // Open file not thread safe, invoke main thread action to open
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        // Did any contents actually change?
                        string diskContents = null;

                        WithFileAccessRetry(() =>
                        {
                            using (var file = File.Open(ActiveDocument.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (var sr = new StreamReader(file, Encoding.Default))
                                {
                                    System.Threading.Thread.Sleep(200);
                                    diskContents = sr.ReadToEnd();//File.ReadAllText(ActiveDocument.FilePath);
                                }
                            }
                        });

                        if (diskContents != ActiveDocument.Document.Text)
                        {
                            // Ask user
                            MessageBoxResult dialogResult = MessageBox.Show(ActiveDocument.FilePath + Environment.NewLine + Environment.NewLine + "This file has been modified by an another program." + Environment.NewLine + Environment.NewLine + "Do you want to reload it?", "Reload", MessageBoxButton.YesNo);

                            if (dialogResult == MessageBoxResult.Yes)
                            {                                
                                ActiveDocument.Document.Text = diskContents;
                            }
                        }
                    }));

                    // Re-enable checks
                    sourceWatcher.EnableRaisingEvents = true;
                }
                System.Threading.Monitor.Exit(watcherCritSec);
            }
        }

        #endregion

        private IHighlightingDefinition GetCurrentHightlighting()
        {
            IHighlightingDefinition definition = HighlightingManager.Instance.GetDefinition("ASM68K");

            switch (configViewModel.SelectedTheme.Item1)
            {
                case "Dark Theme":
                    definition = HighlightingManager.Instance.GetDefinition("ASM68KDark");
                    break;
                case "Light Theme":
                case "Blue Theme":
                    definition = HighlightingManager.Instance.GetDefinition("ASM68K");
                    break;
            }

            return definition;
        }

        #region Open Command Methods

        private bool CanOpen() => true;

        private void OnOpenFile()
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Assembly Files (*.s,*.asm)|*.s;*.asm|All Files (*.*)|*.*";
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                var fileViewModel = Open(dlg.FileName);
            }
        }

        private void OnOpenProjectSolution()
        {
            // TODO: Handle Opening just a project? 
            var dlg = new OpenFileDialog();
            //dlg.Filter = "All Project Files (*.mdsln,*.mdproj)|*.mdsln;*.mdproj|Mega Drive Solution (*.mdsln)|*.mdsln|Mega Drive Project (*.mdproj)|*.mdproj";
            dlg.Filter = "Mega Drive Solution (*.mdsln)|*.mdsln";
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                var file = dlg.FileName;
                var extension = Path.GetExtension(file);
                if (extension == Project.Extension)
                {
                    // open project
                }
                else if (extension == Solution.Extension)
                {
                    OpenSolution(file);
                    configViewModel.Config.LastProject = file;
                    configViewModel.Config.Save();
                }
            }
        }

        #endregion Open Command Methods

        #region New Command Methods

        private bool CanNew() => true;

        private void OnNewSolution()
        {
            // TODO: 
            throw new NotImplementedException();
        }

        private void OnNewProject()
        {
            // Open a New Project Window
        }

        private void OnNewFile()
        {
            var fileViewModel = new FileViewModel();
            fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
            files.Add(fileViewModel);
            ActiveDocument = files.Last();
        }

        #endregion New Command Methods

        private void OnOpenProjectProperties()
        {
            if (isSolutionLoaded)
            {
                ProjectPropertiesView view = new ProjectPropertiesView() { DataContext = new ProjectPropertiesViewModel(solution.CurrentlySelectedProject) };
                view.ShowDialog();
                solution.CurrentlySelectedProject.Save();
            }
        }

        #region Build Command Methods
        private void OnBuild()
        {
            PreBuild();
        }

        private bool CanBuild()
        {
            bool canBuild = solution != null;
            canBuild &= !isBuilding;
            return canBuild;
        }

        #endregion

        #region Run Command Methods
        private void OnRunEmulator()
        {
            if (IsBuilding == false && IsDebugging == false)
            {
                // call run in a background work so we can 
                // still interact with the app
                var worker = new BackgroundWorker();
                worker.DoWork += (sender, e) => Run();
                worker.RunWorkerCompleted += (sender, e) =>
                {
                    Status = "Ready";
                };
                worker.RunWorkerAsync();
            }
            else if(state == State.Paused || state == State.Debugging)
            {
                target.Resume();
                state = State.Running;
                IsBreakPointHit = false;
                ActiveDocument?.RemoveAllMarkers();

                if (target is EmulatorTarget emulator)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        emulator.BringToFront();
                    }));
                }
            }
        }

        private bool CanRunEmulator() => true;

        #endregion

        #region Stop Command Methods
        private void OnStopEmulator()
        {
            if (state != State.Stopped)
            {
                if (target is EmulatorTarget emulator)
                {
                    emulator.Shutdown();
                }

                state = State.Stopping;

                //Restore original breakpoint lines
                var breakpointsCopy = breakpoints.ToArray();
                foreach (var breakpoint in breakpointsCopy)
                {
                    if (breakpoint.OriginalLine > 0)
                    {
                        if (breakpoint.Filename.Equals(ActiveDocument.FilePath, StringComparison.OrdinalIgnoreCase))
                        {
                            if (ActiveDocument.IsBreakpointSet(breakpoint.Line))
                                ActiveDocument?.ToggleBreakpoint(breakpoint.Line);

                            if (!ActiveDocument.IsBreakpointSet(breakpoint.OriginalLine))
                                ActiveDocument?.ToggleBreakpoint(breakpoint.OriginalLine);
                        }

                        breakpoint.Line = breakpoint.OriginalLine;
                        breakpoint.OriginalLine = 0;
                    }
                }

                ActiveDocument?.RemoveAllMarkers();
                ActiveDocument?.Refresh();

                IsDebugging = false;
                state = State.Stopped;
                Status = "Stopped";

                StopDebugging();
            }
        }

        private bool CanStopEmulator() => true;

        #endregion

        #region Restart Emulator Command Methods
        private void OnRestartEmulator()
        {
            if (target is EmulatorTarget emulatorTarget)
            {
                emulatorTarget.SoftReset();
            }
            else
            {
                target.Reset();
            }

            ActiveDocument?.RemoveAllMarkers();
        }
        #endregion

        #region Break All Command Methods
        private void OnBreakAll()
        {
            if (state == State.Running)
            {
                target.Break();
                IsBreakPointHit = true;
                state = State.Paused;
                uint currentPC = target.GetPC();
                GoTo(currentPC);
                UpdateRegisterView(currentPC);
            }
        }
        #endregion

        #region About Command Methods
        private void OnAboutWindowOpen()
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }
        #endregion

        #region Configuration Command Methods
        private void OnConfigurationOpen()
        {
            ConfigView view = new ConfigView();
            view.DataContext = configViewModel;

            if (view.ShowDialog() == true)
            {
                configViewModel = view.DataContext as ConfigViewModel;
                configViewModel.Config.Save();

                if (solution != null)
                {
                    // update the assembler path in case they were changed
                    foreach (var project in solution.Projects)
                    {
                        if (project.Assembler is AsAssembler asm)
                        {
                            asm.AssemblerPath = configViewModel.AsPath;
                        }
                        // TODO: add Asm68KAssembler once the asm68k symbols are fixed
                        //else if(project.Assembler is Asm68kAssembler asm)
                        //{
                        //    asm.AssemblerPath = configViewModel.Asm68kPath;
                        //}
                    }
                }
            }
        }
        #endregion

        #region Breakpoint Command Methods
        private void OnBreakpointAdded(BookmarkEventArgs e)
        {
            
        }

        private void OnBreakpointRemoved(object parameter)
        {
            if (state != State.Stopping)
            {
                var breakpoint = ((BookmarkEventArgs)parameter).Bookmark as BreakpointBookmark;
                var bp = breakpoints.FirstOrDefault(b => b.Filename == breakpoint.FileName && b.Line == breakpoint.LineNumber);
                if (bp != null)
                {
                    RemoveBreakpoint(bp.Filename, bp.Line);
                    breakpoints.Remove(bp);
                }
            }
        }

        private void OnBreakPointBeforeAdded(BookmarkEventArgs e)
        {
            int lineNo = e.Bookmark.LineNumber;

            if (breakpoints.Find(breakpoint => breakpoint.Filename.Equals(ActiveDocument.FilePath, StringComparison.OrdinalIgnoreCase) && breakpoint.Line == lineNo) == null)
            {
                lineNo = SetBreakpoint(ActiveDocument.FilePath, lineNo);

                if (state != State.Stopping)
                {
                    // adjust the breakpoint if necessary
                    e.Bookmark = new BreakpointBookmark(e.Bookmark.FileName, new ICSharpCode.AvalonEdit.Document.TextLocation(lineNo, 0));
                }
                /*if (e.Bookmark.LineNumber > -1)
                {
                    breakpoints.Add(new Breakpoint(e.Bookmark.FileName, e.Bookmark.LineNumber));
                }*/
            }
        }

        private void OnBreakpointBeforeRemoved(BookmarkEventArgs e)
        {
            
        }

        #endregion

        private void OpenSolution(string file)
        {
            if (!String.IsNullOrEmpty(file))
            {
                ResetDocument();
                // open solution 
                solution = new Solution(file);
                // populate the solution explorer...
                explorer.Solution = solution;
                SolutionName = solution.Name;

                Application.Current.Dispatcher.Invoke(new Action(() =>  IsSolutionLoaded = true ));
            }
        }

        private void ResetDocument()
        {
            if (sourceWatcher != null)
            {
                sourceWatcher.Changed -= onFileChanged;
                sourceWatcher = null;
            }

            ActiveDocument = null;
        }

        private int PreBuild()
        {           
            int returnValue = 0;
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                 SaveAll();
                e.Result = Build();
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                returnValue = (int)e.Result;                
            };
            worker.RunWorkerAsync();
            return returnValue;
        }

        private int Build()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Status = "Building...";
                IsBuilding = true;
            }));

            Stopwatch sw = Stopwatch.StartNew();

            // clear the output
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Errors.Clear();
                Output.BuildOutput = "";
                Output.IsVisible = true;
            }));

            // clear error markers
            ErrorMarkers.Clear();
            
            foreach (var file in files)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    file.RemoveAllMarkers();
                }));
            }


            int errorCount = solution.Build();
            sw.Stop();

            // load debugging symbols
            debugSymbols = solution.GetDebugSymbols();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                IsBuilding = false;
            }));

            // display success or fail message
            if (errorCount == 0)
            {
                alreadyBuilt = true;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Output.BuildOutput += $"\nBuild Finished in {sw.Elapsed.Minutes}:{sw.Elapsed.Seconds}:{sw.Elapsed.Milliseconds}";
                    Status = "Build Succeeded!";
                }));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Output.BuildOutput += $"\nBuild Failed.\nThere were {errorCount} Errors.";
                    Status = "Build Failed";
                }));
            }
            

            return errorCount;
        }

        private void Run()
        {
            if (state == State.Debugging)
            {
                // TODO: update code editor
                target.Resume();

                if (target is EmulatorTarget emulator)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        emulator.BringToFront();
                    }));
                }

                state = State.Running;
            }
            else if (state == State.Stopped)
            {
                SaveAll();

                int errorCount = 0;

                if(!alreadyBuilt)
                {
                    errorCount = Build();
                }

                if (alreadyBuilt || errorCount == 0)
                {
                    alreadyBuilt = true;
                    // TODO: Show Registers

                    //Init emu
                    Tuple<string, Point> resolution = configViewModel.SelectedResolution;
                    Point heightWidth = resolution.Item2;

                    /*Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        EmulatorWindow.Show();
                        //EmulatorWindow.Handle = new WindowInteropHelper(EmulatorWindow).Handle;
                    }));*/

                    

                    if (target is EmulatorTarget emulator)
                    {
                        emulator.Initialise((int)heightWidth.X, (int)heightWidth.Y, handle,
                                    ConfigViewModel.Config.Pal, Regions[ConfigViewModel.Config.EmuRegion].Item1);

                        emulator.SetInputMapping(EmulatorInputs.InputUp, ConfigViewModel.Config.KeycodeUp);
                        emulator.SetInputMapping(EmulatorInputs.InputDown, ConfigViewModel.Config.KeycodeDown);
                        emulator.SetInputMapping(EmulatorInputs.InputLeft, ConfigViewModel.Config.KeycodeLeft);
                        emulator.SetInputMapping(EmulatorInputs.InputRight,ConfigViewModel.Config.KeycodeRight);
                        emulator.SetInputMapping(EmulatorInputs.InputA, ConfigViewModel.Config.KeycodeA);
                        emulator.SetInputMapping(EmulatorInputs.InputB, ConfigViewModel.Config.KeycodeB);
                        emulator.SetInputMapping(EmulatorInputs.InputC, ConfigViewModel.Config.KeycodeC);
                        emulator.SetInputMapping(EmulatorInputs.InputStart, ConfigViewModel.Config.KeycodeStart);
                        
                    }
                    
                    state = State.Running;
                    
                    target.LoadBinary(solution.BinaryPath);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        //  Lookup and set initial breakpoints
                        for (int i = 0; i < breakpoints.Count; i++)
                        {
                            breakpoints[i].Address = debugSymbols.GetAddress(breakpoints[i].Filename, breakpoints[i].Line);
                            SetTargetBreakpoint(breakpoints[i].Address);

                            // Move any breakpoints on lines without addresses
                            DebugInfo fileLine = debugSymbols.GetFileLine(breakpoints[i].Filename,breakpoints[i].Address);
                            if (breakpoints[i].Line != fileLine.LineTo)
                            {
                                //Line differs, backup and set real line
                                breakpoints[i].OriginalLine = breakpoints[i].Line;
                                breakpoints[i].Line = fileLine.LineTo;

                                if (breakpoints[i].Filename.Equals(ActiveDocument.FilePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    //Remove all breakpoint carets up to the real line
                                    for (int line = fileLine.LineFrom; line < fileLine.LineTo; line++)
                                    {
                                        if (ActiveDocument.IsBreakpointSet(line))
                                        {
                                            ActiveDocument?.ToggleBreakpoint(line);
                                        }
                                    }

                                    //Set real breakpoint
                                    if (!ActiveDocument.IsBreakpointSet(fileLine.LineTo))
                                    {
                                        ActiveDocument?.ToggleBreakpoint(fileLine.LineTo);
                                    }
                                }
                            }
                        }


                        Status = "Running...";
                        IsDebugging = true;

                        StartDebugging();

                    }));

                    target.Run();
                    
                    // TODO: Breakpoints and watchpoints



                }
            }
        }

        #endregion Private Methods
    }
}