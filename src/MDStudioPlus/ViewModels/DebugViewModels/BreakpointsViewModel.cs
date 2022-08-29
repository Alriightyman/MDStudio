using MDStudioPlus.Models.Debugging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.ViewModels
{
    public class BreakpointsViewModel : ToolViewModel
    {
        private ObservableCollection<Breakpoint> breakpoints = new ObservableCollection<Breakpoint>();
        public const string ToolContentId = "BreakpointsTool";
        private DateTime _lastModified;
        private long _fileSize;
        private string _FileName;
        private string _FilePath;

        public ObservableCollection<Breakpoint> Breakpoints
        {
            get { return breakpoints; }
            set
            {
                breakpoints = value;
                RaisePropertyChanged(nameof(Breakpoints));
            }
        }

        private Breakpoint selectedItem;
        public Breakpoint SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public BreakpointsViewModel() : base("Breakpoints")
        {
            ContentId = ToolContentId;
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
    }
}
