using MDStudioPlus.FileExplorer.Events;
using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MDStudioPlus.FileExplorer
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Item : ViewModelBase
    {
        private RelayCommand renameCommand;
        private RelayCommand lostFocusCommand;
        private string name = "";
        protected string oldName;
        public event SelectedItemEventHandler OnSelectedItemChanged;
        public string Name
        {
            get => name;
            set
            {
                if (name != value && value != String.Empty)
                {
                    oldName = name;
                    name = value;

                    // when this is first set, oldName should be the same as Name
                    if(oldName == String.Empty)
                    {
                        oldName = name;
                    }

                    RaisePropertyChanged(nameof(Name));
                }
            }
        }
        public string Path { get; set; }
        public Item Parent { get; set; }

        public ExplorerViewModel Explorer { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;

                if (OnSelectedItemChanged != null && isSelected == true)
                {
                    OnSelectedItemChanged(this, new SelectedItemEventArgs(this));
                }
            }
        }

        private bool isRenaming;

        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                isRenaming = value;
                RaisePropertyChanged(nameof(IsRenaming));
            }
        }


        public ICommand RenameCommand
        {
            get
            {
                if (renameCommand == null)
                {
                    renameCommand = new RelayCommand((p) => OnRename());
                }
                return renameCommand;
            }
        }
        

        public ICommand LostFocusCommand
        {
            get
            {
                if (lostFocusCommand == null)
                {
                    lostFocusCommand = new RelayCommand((p) => OnLostFocus(p));
                }
                return lostFocusCommand;
            }
        }
        
        public Item()
        {
        }

        private void OnRename()
        {
            IsRenaming = true;
        }

        protected virtual void OnLostFocus(object param)
        {
            IsRenaming = false;
        }
    }
}
