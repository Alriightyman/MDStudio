using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.ViewModels
{
    internal class OutputViewModel : ToolViewModel
    {
        #region fields
        public const string ToolContentId = "OutputTool";
        private DateTime _lastModified;
        private long _fileSize;
        private string _FileName;
        private string _FilePath;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public OutputViewModel()
            : base("Output")
        {
            Workspace.Instance.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
            ContentId = ToolContentId;
        }
        #endregion constructors

        #region Properties

        public AvalonDock.Layout.LayoutAnchorable AnchorablePane { get; set; }

        private string buildOutput;
        public string BuildOutput
        {
            get => buildOutput;
            set
            {                
                buildOutput = value;
                RaisePropertyChanged(nameof(BuildOutput));                
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

        #region methods
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
