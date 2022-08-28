using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using MDStudioPlus.Editor;
using MDStudioPlus.Editor.BookMarks;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Xml;

namespace MDStudioPlus.ViewModels
{
    public class FileViewModelSelectedEventArgs : EventArgs
    {
        public FileViewModel SelectedFile { get; set;}
        public FileViewModelSelectedEventArgs(FileViewModel model)
        {
            SelectedFile = model;
        }
    }

    public delegate void FileViewModelSelectedEventHandler(object sender, FileViewModelSelectedEventArgs e);

    public class FileViewModelIsDirtyEventArgs : EventArgs
    {
        public bool IsDirty { get; set; }
        public FileViewModelIsDirtyEventArgs(bool isDirty)
        {
            IsDirty = isDirty;
        }
    }

    public delegate void FileViewModelIsDirtyEventHandler(object sender, FileViewModelIsDirtyEventArgs e);


    public class FileViewModel : PaneViewModel
    {
        public event FileViewModelSelectedEventHandler OnFileSelected;
        public event FileViewModelIsDirtyEventHandler OnFileDirty;

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
        public FileViewModel(string filePath, Project project)
        {            
            FilePath = filePath;
            Title = FileName;
            Document = new TextDocument();
            Project = project;
            OpenFile();
            IsDirty = false;
        }

        /// <summary>
        /// Default class constructor
        /// </summary>
        public FileViewModel()
        {
            IsDirty = true;
            Title = FileName;
            Document = new TextDocument();
            Project = null;
        }

        #endregion constructors

        private void Document_TextChanged(object sender, System.EventArgs e)
        {
            IsDirty = true;
        }

        private void OpenFile()
        {
            if (File.Exists(filePath))
            {
                //document = new TextDocument();//Editor.Document;
                using (FileStream fs = new FileStream(this.filePath,
                            FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
                    {
                        //document = new TextDocument(reader.ReadToEnd());
                        document.Text = reader.ReadToEnd();
                       /* document.TextChanged -= Document_TextChanged;
                        document.TextChanged += Document_TextChanged;*/
                    }
                }

                document.FileName = filePath;
                ContentId = filePath;
            }
        }

        #region Properties

        public Project Project { get; set; }

        public CodeEditor Editor  => Document?.ServiceProvider?.GetService(typeof(CodeEditor)) as CodeEditor;

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


        public string FileName
        {
            get
            {
                if (FilePath == null)
                    return "Noname" + (IsDirty ? "*" : "");

                return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
            }
        }

        public override bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                    OnFileSelected?.Invoke(this, new FileViewModelSelectedEventArgs(this));
                }
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
                    document.TextChanged -= Document_TextChanged;
                    document.TextChanged += Document_TextChanged;
                    RaisePropertyChanged(nameof(Document));
                    //IsDirty = true;
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
                    OnFileDirty?.Invoke(this, new FileViewModelIsDirtyEventArgs(isDirty));
                }
            }
        }

        public TextEditorOptions CodeEditorOptions
        {
            get => Workspace.Instance.CodeEditorOptions;
            set
            {
                Workspace.Instance.CodeEditorOptions = value;
                RaisePropertyChanged(nameof(CodeEditorOptions));
            }
        }
        public FontFamily EditorFont
        {
            get => Workspace.Instance.EditorFont;
            set
            {
                Workspace.Instance.EditorFont = value;
                RaisePropertyChanged(nameof(EditorFont));
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

        #endregion Properties

        #region Public Methods

        public bool IsBreakpointSet(int lineNumber)
        {
            CodeEditor codeEditor = (CodeEditor)Document.ServiceProvider.GetService(typeof(CodeEditor));
            return codeEditor.IsBreakpointSet(lineNumber);
        }

        public void ToggleBreakpoint(int line)
        {
            CodeEditor codeEditor = (CodeEditor)Document.ServiceProvider.GetService(typeof(CodeEditor));
            codeEditor?.ToggleBreakpoint(line);
        }

        public void AddMarker(int lineNumber, TextMarkerTypes markerType, Color markerColor)
        {
            CodeEditor codeEditor = (CodeEditor)Document.ServiceProvider.GetService(typeof(CodeEditor));
            codeEditor?.AddMarker(lineNumber, markerType, markerColor);
        }

        public void RemoveAllMarkers()
        {
            CodeEditor codeEditor = (CodeEditor)Document.ServiceProvider.GetService(typeof(CodeEditor));
            codeEditor?.RemoveAllMarkers();
        }

        public void Refresh()
        {
            CodeEditor codeEditor = (CodeEditor)Document.ServiceProvider.GetService(typeof(CodeEditor));
            codeEditor?.Refresh();
        }

        // TODO: This may be used for showing variable/ram information on hover
        public string SetWordAtMousePosition(MouseEventArgs e)
        {
            
            var mousePosition = Editor.GetPositionFromPoint(e.GetPosition(Editor));
            
            if (mousePosition == null)
                return String.Empty;
            
            var line = mousePosition.Value.Line;
            var column = mousePosition.Value.Column;
            var offset = Document.GetOffset(line,column);

            if (offset >= Document.TextLength)
                offset--;

            int offsetStart = TextUtilities.GetNextCaretPosition(Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
            int offsetEnd = TextUtilities.GetNextCaretPosition(Document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

            if (offsetEnd == -1 || offsetStart == -1)
                return String.Empty;

            var currentChar = Document.GetText(offset, 1);

            if (string.IsNullOrWhiteSpace(currentChar))
                return String.Empty;

            var word = Document.GetText(offsetStart, offsetEnd - offsetStart);
            return word;
        }


        #endregion

        #region Private Methods
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
