using MDStudioPlus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MDStudioPlus.ViewModels
{
    internal class ErrorViewModel : ToolViewModel
    {
        #region fields
        public const string ToolContentId = "FileStatsTool";
        private DateTime _lastModified;
        private long _fileSize;
        private string _FileName;
        private string _FilePath;
        private ICommand clearSelection;

        private ObservableCollection<Error> errors = new ObservableCollection<Error>();

        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public ErrorViewModel()
            : base("Error List")
        {
            Workspace.Instance.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
            ContentId = ToolContentId;
        }
        #endregion constructors

        #region Properties
        /// <summary>
        /// Holds the errors for this document
        /// </summary>
        public ObservableCollection<Error> Errors
        {
            get => errors;
            set
            {
                errors = value;
                RaisePropertyChanged(nameof(Errors));
            }
        }

        private Error selectedItem;
        public Error SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                if (selectedItem != null)
                {
                    Workspace.Instance.GoTo(selectedItem.Filename, Int32.Parse(selectedItem.LineNumber), true);
                }
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public long FileSize
        {
            get => _fileSize;
            protected set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    RaisePropertyChanged(nameof(FileSize));
                }
            }
        }

        public DateTime LastModified
        {
            get => _lastModified;
            protected set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    RaisePropertyChanged(nameof(LastModified));
                }
            }
        }

        public string FileName
        {
            get => _FileName;
            protected set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    RaisePropertyChanged(nameof(FileName));
                }
            }
        }

        public string FilePath
        {
            get => _FilePath;
            protected set
            {
                if (_FilePath != value)
                {
                    _FilePath = value;
                    RaisePropertyChanged(nameof(FilePath));
                }
            }
        }

        #endregion Properties

        #region Commands
        public ICommand ClearSelection
        {
            get
            {
                if (clearSelection == null) 
                {
                    clearSelection = new RelayCommand((p) => CheckClearSelection(p));
                }

                return clearSelection;
            }

        }
        #endregion

        #region methods


        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void Update(string line)
        {

            foreach (var project in Workspace.Instance.Solution.Projects)
            {

                var pattern = project.ErrorPattern;

                Match matchError = Regex.Match(line, pattern);

                if (matchError.Success)
                {
                    Error error = new Error();
                    
                    int lineNumber;
                    string filename = matchError.Groups[1].Value;
                    Int32.TryParse(matchError.Groups[2].Value, out lineNumber);
                    string code = matchError.Groups[3].Value;
                    string description = matchError.Groups[4].Value;

                    error.LineNumber = $"{lineNumber}";
                    error.Filename = filename;
                    error.Description = description;
                    error.Code = code;

                    
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Errors.Add(error);
                        Workspace.Instance.AddErrorMarkerToDocument(project, error);
                    }));
                    break;
                }
            }
        }

        /// <summary>
        /// Clears the Errors list
        /// </summary>
        public void Clear()
        {
            Errors.Clear();
        }

        // parameter should be a ListView
        private void CheckClearSelection(object parameter)
        {
            var listView = parameter as ListView;
            if (listView != null)
            {
                listView.SelectedItem = null;
            }
        }

        private void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (Workspace.Instance.ActiveDocument != null &&
                Workspace.Instance.ActiveDocument.FilePath != null &&
                File.Exists(Workspace.Instance.ActiveDocument.FilePath))
            {
                var fi = new FileInfo(Workspace.Instance.ActiveDocument.FilePath);
                FileSize = fi.Length;
                LastModified = fi.LastWriteTime;
                FileName = fi.Name;
                FilePath = fi.Directory.FullName;
            }
            else
            {
                FileSize = 0;
                LastModified = DateTime.MinValue;
                FileName = string.Empty;
                FilePath = string.Empty;
            }
        }
        #endregion methods

    }
}
