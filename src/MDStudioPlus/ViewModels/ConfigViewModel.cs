using AvalonDock.Themes;
using ICSharpCode.AvalonEdit;
using MDStudioPlus.Targets;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MDStudioPlus.ViewModels
{
    public class SelectThemeEventArgs : EventArgs
    {
        public Theme Theme { get; set; }
        public string Name { get; set; }
        public SelectThemeEventArgs(Theme theme, string name)
        {
            this.Theme = theme;
            this.Name = name;
        }
    }

    public delegate void SelectedThemeEventHandler(object sender, SelectThemeEventArgs e);


    class ConfigViewModel : ViewModelBase
    {
        public event SelectedThemeEventHandler OnThemeChanged;
        private Tuple<string, Theme> selectedTheme;
        private Tuple<string, Point> selectedResolution;
        private bool autoOpenLastProject;
        private Tuple<string, string> selectedTarget;
        private Tuple<char, string> selectedRegion;
        private Config config = new Config();
        private string asm68kPath;
        private string asPath;
        private ICommand openAsm68kCommand;
        private ICommand openAsCommand;
        private TextEditorOptions options = new TextEditorOptions();
        public TextEditorOptions EditorOptions
        {
            get => options;
            
            set
            {
                options = value;
                RaisePropertyChanged(nameof(EditorOptions));
            }
        }
        public Config Config
        {
            get => config;
            set
            {
                config = value;
            }
        }

        public FontFamily Font
        {
            get => new FontFamily(config.Font);
            set
            {
                config.Font = value.Source;
                RaisePropertyChanged(nameof(Font));
            }
        }

        public int TabSize
        {
            get => config.TabSize;
            set
            {
                config.TabSize = value;
                RaisePropertyChanged(nameof(TabSize));
                options.IndentationSize = value;
                RaisePropertyChanged(nameof(EditorOptions));
            }
        }

        #region Themes
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
                Config.Theme = selectedTheme.Item1;
                SwitchExtendedTheme();
                RaisePropertyChanged(nameof(SelectedTheme));
            }
        }

        #endregion

        #region Targets
        public List<Tuple<string,string>> Targets { get; set; } = TargetFactory.GetTargetNames();

        public Tuple<string, string> SelectedTarget
        {
            get => selectedTarget;
            set
            {
                selectedTarget = value;
                Config.TargetName = value.Item1;
                RaisePropertyChanged(nameof(SelectedTarget));
            }
        }

        /// <summary>
        /// Gets the selected Target as a tuple. 
        /// Item1 is is the name of the emulator and item2 is the namespace
        /// Item2 is assumed that all Targets will be assigned to MDStudioPlus.Targets
        /// </summary>
        public Tuple<string, string> Target => new Tuple<string, string>(selectedTarget.Item1, selectedTarget.Item2);

        #endregion

        #region Resolution
        public List<Tuple<string, Point>> ValidResolutions { get; set; } = new List<Tuple<string, Point>>(new[]
        {
            new Tuple<string,Point>("320x240",new Point( 320,  240 )),
            new Tuple<string,Point>("640x480",new Point( 640,  480 )),
            new Tuple<string,Point>("960x720",new Point( 960,  720 )),
            new Tuple<string,Point>("1280x960",new Point( 1280, 960 )),
        });

        public Tuple<string, Point> SelectedResolution
        {
            get => selectedResolution;
            set
            {
                selectedResolution = value;
                Config.EmuResolution = selectedResolution.Item1;

                RaisePropertyChanged(nameof(SelectedResolution));
            }
        }

        #endregion

        #region Region
        public List<Tuple<char, string>> Regions { get; set; } = new List<Tuple<char, string>>(new[]
        {
            new Tuple<char, string>( 'J', "Japan" ),
            new Tuple<char, string>( 'U', "USA" ),
            new Tuple<char, string>( 'E', "Europe" )
        });

        public Tuple<char, string> SelectedRegion
        {
            get => selectedRegion;
            set
            {
                selectedRegion = value;
                switch (value.Item1)
                {
                    case 'U':
                        config.EmuRegion = 1;
                        break;
                    case 'J':
                        config.EmuRegion = 0;
                        break;
                    case 'E':
                        config.EmuRegion = 2;
                        break;
                }
                RaisePropertyChanged(nameof(SelectedRegion));
            }
        }
        #endregion

        #region Auto Open Last Project
        public bool AutoOpenLastProject
        {
            get => autoOpenLastProject;
            set
            {
                autoOpenLastProject = value;
                config.AutoOpenLastProject = value;
                RaisePropertyChanged(nameof(AutoOpenLastProject));
            }
        }
        #endregion

        #region Assembler Paths
        public string Asm68kPath
        {
            get => asm68kPath;
            set
            {
                asm68kPath = value;
                Config.Asm68kPath = value;
                RaisePropertyChanged(nameof(Asm68kPath));
            }
        }

        public string AsPath
        {
            get => asPath;
            set
            {
                asPath = value;
                Config.AsPath = value;
                RaisePropertyChanged(nameof(AsPath));
            }
        }
        #endregion

        #region Commands
        public ICommand OpenAsm68kCommand
        {
            get
            {
                if(openAsm68kCommand == null)
                {
                    openAsm68kCommand = new RelayCommand((p) => OnOpenAsm68kPath(), (p) => true);
                }

                return openAsm68kCommand;
            }
        }

        private void OnOpenAsm68kPath()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "AS Assembler (.exe)|*.exe";
            if(ofd.ShowDialog() == true)
            {
                Asm68kPath = ofd.FileName;
            }
        }

        public ICommand OpenAsCommand
        {
            get
            {
                if (openAsCommand == null)
                {
                    openAsCommand = new RelayCommand((p) => OnOpenAsPath(), (p) => true);
                }

                return openAsCommand;
            }
        }

        private void OnOpenAsPath()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "AS Assembler (.exe)|*.exe";
            if (ofd.ShowDialog() == true)
            {
                AsPath = ofd.FileName;
            }
        }

        public ICommand AcceptCommand
        {
            get => new RelayCommand((p) => CloseWindow(p), (p) => true);
        }

        private void CloseWindow(object parameter)
        {
            Window window = (Window)parameter;
            window.DialogResult = true;
            window.Close();
        }

        #endregion

        #region Emulator Controls
        public IEnumerable<SDL_Keycode.Keycode> Keycodes => Enum.GetValues(typeof(SDL_Keycode.Keycode)).Cast<SDL_Keycode.Keycode>();
        public SDL_Keycode.Keycode UpKey 
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeUp;
            set
            {
                Config.KeycodeUp = (int)value;
                RaisePropertyChanged(nameof(UpKey));
            }
        }

        public SDL_Keycode.Keycode DownKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeDown;
            set
            {
                Config.KeycodeDown = (int)value;
                RaisePropertyChanged(nameof(DownKey));
            }
        }

        public SDL_Keycode.Keycode LeftKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeLeft;
            set
            {
                Config.KeycodeLeft = (int)value;
                RaisePropertyChanged(nameof(LeftKey));
            }
        }

        public SDL_Keycode.Keycode RightKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeRight;
            set
            {
                Config.KeycodeRight = (int)value;
                RaisePropertyChanged(nameof(RightKey));
            }
        }

        public SDL_Keycode.Keycode AKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeA;
            set
            {
                Config.KeycodeA = (int)value;
                RaisePropertyChanged(nameof(AKey));
            }
        }

        public SDL_Keycode.Keycode BKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeB;
            set
            {
                Config.KeycodeB = (int)value;
                RaisePropertyChanged(nameof(BKey));
            }
        }

        public SDL_Keycode.Keycode CKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeC;
            set
            {
                Config.KeycodeC = (int)value;
                RaisePropertyChanged(nameof(CKey));
            }
        }

        public SDL_Keycode.Keycode StartKey
        {
            get => (SDL_Keycode.Keycode)Config.KeycodeStart;
            set
            {
                Config.KeycodeStart= (int)value;
                RaisePropertyChanged(nameof(StartKey));
            }
        }

        #endregion

        public ConfigViewModel()
        {
            
        }

        public void Initialize()
        {
            Config.Read();

            SelectedTheme = Themes.FirstOrDefault(t => t.Item1 == config.Theme);

            switch (config.EmuRegion)
            {
                case 0:
                    SelectedRegion = Regions.First();
                    break;
                case 1:
                    selectedRegion = Regions.First(r => r.Item1 == 'U');
                    break;
                case 2:
                    selectedRegion = Regions.Last();
                    break;
            }
            SelectedResolution = ValidResolutions.First(r => r.Item1 == Config.EmuResolution);
            SelectedTarget = new Tuple<string, string>(Config.TargetName, $"{nameof(MDStudioPlus)}.{nameof(Targets)}");
            AutoOpenLastProject = Config.AutoOpenLastProject;
            AsPath = Config.AsPath;
            Asm68kPath = Config.Asm68kPath;
            options.IndentationSize = config.TabSize;
        }

        public void SwitchExtendedTheme()
        {
            switch (selectedTheme.Item1)
            {
                case "Dark Theme":
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/MLib;component/Themes/DarkTheme.xaml");
                    Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/MDStudioPlus;component/Themes/DarkBrushsExtended.xaml");
                    break;
                case "Light Theme":
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/MLib;component/Themes/LightTheme.xaml");
                    Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/MDStudioPlus;component/Themes/LightBrushsExtended.xaml");
                    break;
                case "Blue Theme":
                    //TODO: Create new color resources for blue theme
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/MLib;component/Themes/LightTheme.xaml");
                    Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/MDStudioPlus;component/Themes/BlueBrushsExtended.xaml");
                    break;
                default:
                    break;
            }
            OnThemeChanged?.Invoke(this, new SelectThemeEventArgs(selectedTheme.Item2, selectedTheme.Item1));
        }
    }
}
