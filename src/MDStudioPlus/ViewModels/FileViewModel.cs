using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Xml;

namespace MDStudioPlus.ViewModels
{
    class FileViewModel : PaneViewModel
    {
        #region fields
        private string filePath = null;
        private TextDocument document;
        private IHighlightingDefinition syntaxHighlightingName;
        private bool isDirty = false;
        private RelayCommand saveCommand = null;
        private RelayCommand saveAsCommand = null;
        private RelayCommand closeCommand = null;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor from file path.
        /// </summary>
        /// <param name="filePath"></param>
        public FileViewModel(string filePath)
        {
            FilePath = filePath;
            Title = FileName;
            OpenFile();
        }

        /// <summary>
        /// Default class constructor
        /// </summary>
        public FileViewModel()
        {
            IsDirty = true;
            Title = FileName;
            document = new TextDocument();
            document.TextChanged -= Document_TextChanged;
            document.TextChanged += Document_TextChanged;            
        }

        #endregion constructors

        #region Properties

        public IHighlightingDefinition SyntaxHighlightName
        {
            get => syntaxHighlightingName;
            set
            {
                syntaxHighlightingName = value;
                RaisePropertyChanged(nameof(SyntaxHighlightName));
            }
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    RaisePropertyChanged(nameof(FilePath));
                    RaisePropertyChanged(nameof(FileName));
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        private void Document_TextChanged(object sender, System.EventArgs e)
        {
            IsDirty = true;
        }

        public string FileName
        {
            get
            {
                if (FilePath == null)
                    return "Noname" + (IsDirty ? "*" : "");

                return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
            }
        }

        private void OpenFile()
        {
            if (File.Exists(filePath))
            {
                document = new TextDocument();//Editor.Document;
                using (FileStream fs = new FileStream(this.filePath,
                            FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
                    {
                        document = new TextDocument(reader.ReadToEnd());
                        document.TextChanged -= Document_TextChanged;
                        document.TextChanged += Document_TextChanged;
                    }
                }

                document.FileName = filePath;
                ContentId = filePath;
            }
        }
        public TextDocument Document
        {
            get { return document; }
            set
            {
                if (document != value)
                {
                    document = value;
                    RaisePropertyChanged(nameof(Document));
                    IsDirty = true;
                }
            }
        }

        public bool IsDirty
        {
            get => isDirty;
            set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    Title = FileName;
                    RaisePropertyChanged(nameof(IsDirty));
                    RaisePropertyChanged(nameof(FileName));
                }
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
                }

                return saveCommand;
            }
        }

        public ICommand SaveAsCommand
        {
            get
            {
                if (saveAsCommand == null)
                {
                    saveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
                }

                return saveAsCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (closeCommand == null)
                {
                    closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
                }

                return closeCommand;
            }
        }

        #endregion  Properties

        #region methods
        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            Workspace.Instance.Close(this);
        }

        private bool CanSave(object parameter)
        {
            return IsDirty;
        }

        private void OnSave(object parameter)
        {
            Workspace.Instance.Save(this, false);
        }

        private bool CanSaveAs(object parameter)
        {
            return IsDirty;
        }

        private void OnSaveAs(object parameter)
        {
            Workspace.Instance.Save(this, true);
        }
        #endregion methods
    }
}
