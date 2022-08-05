using System;

namespace MDStudioPlus.FileExplorer.Events
{
    public class ExpandedItemEvent : EventArgs
    {
        public DirectoryItemViewModel SelectedItem { get; set; }
        public ExpandedItemEvent(DirectoryItemViewModel selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }

    public delegate void ExpandedItemEventHandler(object sender, ExpandedItemEvent e);
}
