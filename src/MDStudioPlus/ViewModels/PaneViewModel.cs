using System.Windows.Media;

namespace MDStudioPlus.ViewModels
{
    class PaneViewModel : ViewModelBase
    {
        #region fields
        private string title = null;
        private string contentId = null;
        private bool isSelected = false;
        private bool isActive = false;
        #endregion fields

        public PaneViewModel()
        {

        }

        public string Title
        {
            get => title;
            set
            {
                if(title != value)
                {
                    title = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public ImageSource IconSource { get; protected set; }

        public string ContentId
        {
            get => contentId;
            set
            {
                if (contentId != value)
                {
                    contentId = value;
                    RaisePropertyChanged(nameof(ContentId));
                }
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    RaisePropertyChanged(nameof(IsActive));
                }
            }
        }
    }
}
