using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer.Events
{
    public class SelectedItemEventArgs : EventArgs
    {
        public ItemViewModel SelectedItem { get; set; }
        public SelectedItemEventArgs(ItemViewModel selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }

    public delegate void SelectedItemEventHandler(object sender, SelectedItemEventArgs e);

}
