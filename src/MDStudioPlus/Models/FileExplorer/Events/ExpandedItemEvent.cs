using System;

namespace MDStudioPlus.FileExplorer.Events
{
    public class ExpandedItemEvent : EventArgs
    {
        public DirectoryItem SelectedItem { get; set; }
        public ExpandedItemEvent(DirectoryItem selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }

    public delegate void ExpandedItemEventHandler(object sender, ExpandedItemEvent e);
}
