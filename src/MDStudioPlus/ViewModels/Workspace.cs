using AvalonDock.Themes;
using ICSharpCode.AvalonEdit.Highlighting;
using MDStudioPlus.Editor;
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
        #endregion

        #region Fields
        private static Workspace instance = new Workspace();
        public static Workspace Instance => instance;

        private IntPtr handle;

        private ToolViewModel[] tools;
        private ObservableCollection<FileViewModel> files = new ObservableCollection<FileViewModel>();
        private ReadOnlyObservableCollection<FileViewModel> readonyFiles;

        private FileViewModel activeDocument;
        private ErrorViewModel errors;
        private ExplorerViewModel explorer;
        private OutputViewModel output;
        private ConfigViewModel configViewModel;
        private SolidColorBrush statusBackgroundColor;
        private RelayCommand openProjectSolutionCommand;
        private RelayCommand openFileCommand;
        private RelayCommand newSolutionCommand;
        private RelayCommand newProjectCommand;
        private RelayCommand newFileCommand;
        private RelayCommand saveFileCommand;
        private RelayCommand saveAllCommand;
        private RelayCommand buildSolutionCommand;
        private RelayCommand runEmulatorCommand;
        private RelayCommand stopEmulatorCommand;
        private RelayCommand configurationCommand;
        private RelayCommand aboutCommand;
        private Tuple<string, Theme> selectedTheme;

        private State state;
        private FileSystemWatcher m_SourceWatcher;
        private FileSystemEventHandler m_OnFileChanged;
        private object m_WatcherCritSec = new object();
        private bool isDebugging = false;

        // emulator
        private Target target;
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public static readonly ReadOnlyCollection<Tuple<int, int>> ValidResolutions = new ReadOnlyCollection<Tuple<int, int>>(new[]
        {
            new Tuple<int,int>( 320, 240 ),
            new Tuple<int,int>( 640, 480 ),
            new Tuple<int,int>( 960, 720 ),
            new Tuple<int,int>( 1280, 720 ),
        });

        public static readonly ReadOnlyCollection<Tuple<char, string>> Regions = new ReadOnlyCollection<Tuple<char, string>>(new[]
        {
            new Tuple<char, string>( 'J', "Japan" ),
            new Tuple<char, string>( 'U', "USA" ),
            new Tuple<char, string>( 'E', "Europe" )
        });

        // project/solution
        private Solution solution;
        private bool isLoaded = false;
        // building
        private bool alreadyBuilt = false;
        private bool isBuilding = false;

        private string solutionName = "No Project Opened";
        private string status = "Ready";
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

            //try
            {
                //target = TargetFactory.Create();
                target = new TargetDGen();
                // updat config
            }

            timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;            
        }

        private void ConfigViewModel_OnThemeChanged(object sender, SelectThemeEventArgs e)
        {
            SelectedTheme = Themes.FirstOrDefault(t => t.Item1 == e.Name);
            //SwitchExtendedTheme();
        }

        #endregion

        #region Properties

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

        /// <summary>
        /// Set when a Solution has been loaded
        /// </summary>
        public bool IsLoaded
        {
            get => isLoaded;
            set
            {
                isLoaded = value;
                RaisePropertyChanged(nameof(IsLoaded));
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
                }
                else
                {
                    StatusBackgroundColor = (SolidColorBrush)Application.Current.Resources["StatusBarBackground"];
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
                    tools = new ToolViewModel[] { Explorer, Errors, Output };
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
                    activeDocument = value;

                    RaisePropertyChanged(nameof(ActiveDocument));
                    if (ActiveDocumentChanged != null)
                        ActiveDocumentChanged(this, EventArgs.Empty);
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
                SwitchExtendedTheme();
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
                    openProjectSolutionCommand = new RelayCommand((p) => OnOpenProjectSolution(p), (p) => CanOpen(p));
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
                    openFileCommand = new RelayCommand((p) => OnOpenFile(p), (p) => CanOpen(p));
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
                    newSolutionCommand = new RelayCommand((p) => OnNewSolution(p), (p) => CanNew(p));
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
                    newProjectCommand = new RelayCommand((p) => OnNewProject(p), (p) => CanNew(p));
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
                    newFileCommand = new RelayCommand((p) => OnNewFile(p), (p) => CanNew(p));
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
                    buildSolutionCommand = new RelayCommand((p) => OnBuild(p), (p) => CanBuild(p));
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
                    runEmulatorCommand = new RelayCommand((p) => OnRunEmulator(p), (p) => CanRunEmulator(p));
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
                    stopEmulatorCommand = new RelayCommand((p) => OnStopEmulator(p), (p) => CanStopEmulator(p));
                }

                return stopEmulatorCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (saveFileCommand == null)
                {
                    saveFileCommand = new RelayCommand((p) => OnSave(p), (p) => true);
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
                    saveAllCommand = new RelayCommand((p) => OnSaveAll(p), (p) => true);
                }

                return saveAllCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if(aboutCommand == null)
                {
                    aboutCommand = new RelayCommand((p) => OnAboutWindowOpen(p), (p) => true);
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
                    configurationCommand = new RelayCommand((p) => OnConfigurationOpen(p), (p) => true);
                }

                return configurationCommand;
            }
        }
        #endregion

        #region Events
        private void Explorer_OnSelectedItemChanged(object sender, FileExplorer.SelectedItemEventArgs e)
        {
            if (e.SelectedItem is FileExplorer.FileItem || e.SelectedItem is FileExplorer.ProjectItem)
            {
                var fileViewModel = Open(e.SelectedItem.Path);
                fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
                ActiveDocument = fileViewModel;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((target != null && target.IsHalted())
                && state == State.kRunning)
            {
                // TODO debugging
            }
            else
            {
                // TODO update debug windows
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
        public void SwitchExtendedTheme()
        {           
            if (activeDocument != null) activeDocument.SyntaxHighlightName = GetCurrentHightlighting();
        }
        #endregion

        #region Private Methods

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

            files.Remove(fileToClose);
        }

        internal void OnSave(object parameter)
        {
            Save(ActiveDocument);
        }

        internal void OnSaveAll(object parameter)
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
            string newTitle = string.Empty;
            if (fileToSave.FilePath == null || saveAsFlag)
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
                File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);

                ActiveDocument.IsDirty = false;

                if (string.IsNullOrEmpty(newTitle)) return;
                ActiveDocument.Title = newTitle;
            }
        }

        internal FileViewModel Open(string filepath)
        {
            var fileViewModel = files.FirstOrDefault(fm => fm.FilePath == filepath);
            if (fileViewModel != null)
                return fileViewModel;

            fileViewModel = new FileViewModel(filepath);
            fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
            files.Add(fileViewModel);
            return fileViewModel;
        }

        private IHighlightingDefinition GetCurrentHightlighting()
        {
            switch (configViewModel.SelectedTheme.Item1)
            {
                case "Dark Theme":
                    return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("ASM68KDark");
                case "Light Theme":
                case "Blue Theme":
                    return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("ASM68K");
            }

            return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("ASM68K");
        }

        #region Open Command Methods

        private bool CanOpen(object parameter) => true;

        private void OnOpenFile(object parameter)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Assembly Files (*.s,*.asm)|*.s;*.asm|All Files (*.*)|*.*";
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                var fileViewModel = Open(dlg.FileName);
                ActiveDocument = fileViewModel;
            }
        }

        private void OnOpenProjectSolution(object parameter)
        {
            // TODO: 
            var dlg = new OpenFileDialog();
            dlg.Filter = "All Project Files (*.mdsln,*.mdproj)|*.mdsln;*.mdproj|Mega Drive Solution (*.mdsln)|*.mdsln|Mega Drive Project (*.mdproj)|*.mdproj";
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
                }
            }
        }

        #endregion Open Command Methods

        #region New Command Methods

        private bool CanNew(object parameter) => true;

        private void OnNewSolution(object parameter)
        {
            // TODO: 
            throw new NotImplementedException();
        }

        private void OnNewProject(object parameter)
        {
            // Open a New Project Window
        }

        private void OnNewFile(object parameter)
        {
            var fileViewModel = new FileViewModel();
            fileViewModel.SyntaxHighlightName = GetCurrentHightlighting();
            files.Add(fileViewModel);
            ActiveDocument = files.Last();
        }

        #endregion New Command Methods

        #region Build Command Methods
        private void OnBuild(object parameter)
        {
            PreBuild();
        }

        private bool CanBuild(object parameter)
        {
            bool canBuild = solution != null;
            canBuild &= !isBuilding;
            return canBuild;
        }

        #endregion

        #region Run Command Methods
        private void OnRunEmulator(object parameter)
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

        private bool CanRunEmulator(object parameter) => true;

        #endregion

        #region Stop Command Methods
        private void OnStopEmulator(object parameter)
        {
            if (state != State.kStopped)
            {
                if (target is EmulatorTarget emulator)
                {
                    emulator.Shutdown();
                }
                IsDebugging = false;
                state = State.kStopped;
                Status = "Stopped";
            }
        }

        private bool CanStopEmulator(object parameter) => true;

        #endregion

        #region About Command Methods
        private void OnAboutWindowOpen(object parameter)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }
        #endregion

        #region Configuration Command Methods
        private void OnConfigurationOpen(object parameter)
        {
            ConfigView view = new ConfigView();
            view.DataContext = configViewModel;
            view.ShowDialog();
            configViewModel = view.DataContext as ConfigViewModel;
            configViewModel.Config.Save();
        }
        #endregion

        private void OpenSolution(string file)
        {
            // open solution 
            solution = new Solution(file);
            // populate the solution explorer...
            explorer.Solution = solution;
            SolutionName = solution.Name;
            IsLoaded = true;
        }

        private int PreBuild()
        {
            Status = "Building...";
            int returnValue = 0;
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (sender, e) => e.Result = Build();
            worker.RunWorkerCompleted += (sender, e) =>
            {
                returnValue = (int)e.Result;                
            };
            worker.RunWorkerAsync();
            return returnValue;
        }

        private int Build()
        {
            isBuilding = true;
            Stopwatch sw = Stopwatch.StartNew();
            /*string assemblerPath = m_Project.Assembler == Assembler.AS ? m_Config.AsPath : m_Config.Asm68kPath;

            if (assemblerPath == null || assemblerPath.Length == 0)
            {
                Action messageAction = () =>
                {
                    MessageBox.Show("Assembler Path not set\nPlease set it in the Config menu.");
                    configMenu_Click(this, null);
                };
                BeginInvoke(messageAction);

                return 0;
            }*/

            /*if (!File.Exists(assemblerPath))
            {
                MessageBox.Show("Cannot find '" + assemblerPath + "'");
                return 0;
            }*/


            // clear the output
            Output.BuildOutput = "";
            Output.IsVisible = true;

            int errorCount = solution.Build();

            isBuilding = false;

            if (errorCount == 0)
            {
                Output.BuildOutput += "\nBuild Finished";
                alreadyBuilt = true;
                Status = "Build Succeeded..";
            }
            else
            {
                Output.BuildOutput += $"\nBuild Failed.\nThere were {errorCount} Errors.";
                Status = "Build Failed";
            }

            return errorCount;
        }

        private void Run()
        {
            if (state == State.kDebugging)
            {
                // TODO: update code editor
                target.Resume();

                if (target is EmulatorTarget emulator)
                {
                    emulator.BringToFront();

                }

                state = State.kRunning;
            }
            else if (state == State.kStopped)
            {
                SaveAll();

                if (alreadyBuilt || Build() == 0)
                {
                    alreadyBuilt = true;
                    // TODO: Show Registers

                    //Init emu
                    Tuple<string, Point> resolution = configViewModel.SelectedResolution;

                    if (target is EmulatorTarget emulator)
                    {
                        Point heightWidth = resolution.Item2;
                        emulator.Initialise((int)heightWidth.X, (int)heightWidth.Y, handle,
                                    ConfigViewModel.Config.Pal, Regions[ConfigViewModel.Config.EmuRegion].Item1);

                        emulator.SetInputMapping(EmulatorInputs.InputUp, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeUp));
                        emulator.SetInputMapping(EmulatorInputs.InputDown, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeDown));
                        emulator.SetInputMapping(EmulatorInputs.InputLeft, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeLeft));
                        emulator.SetInputMapping(EmulatorInputs.InputRight, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeRight));
                        emulator.SetInputMapping(EmulatorInputs.InputA, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeA));
                        emulator.SetInputMapping(EmulatorInputs.InputB, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeB));
                        emulator.SetInputMapping(EmulatorInputs.InputC, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeC));
                        emulator.SetInputMapping(EmulatorInputs.InputStart, (int)Enum.GetValues(typeof(SDL_Keycode.Keycode)).GetValue(ConfigViewModel.Config.KeycodeStart));
                        
                    }


                    target.LoadBinary(solution.BinaryPath);
                    
                    // TODO: Breakpoints and watchpoints


                    state = State.kRunning;
                    Status = "Running...";
                    IsDebugging = true;
                    target.Run();

                }
            }
        }
        #endregion Private Methods
    }
}
