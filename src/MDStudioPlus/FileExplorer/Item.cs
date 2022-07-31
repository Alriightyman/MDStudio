using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    public class SelectedItemEventArgs : EventArgs
    {
        public Item SelectedItem { get; set; }
        public SelectedItemEventArgs(Item selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }

    public delegate void SelectedItemEventHandler(object sender, SelectedItemEventArgs e);

    public class Item
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
