using MDStudioPlus.FileExplorer.Events;
using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Item : ViewModelBase
    {
        public event SelectedItemEventHandler OnSelectedItemChanged;
        public string Name { get; set; }
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
    }
}
