using System.Windows.Controls;

namespace MDStudioPlus
{
    public static class Extensions
    {

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string st)
        {
            return string.IsNullOrWhiteSpace(st);
        }

        public static void ExpandAll(this TreeView treeView)
        {
            foreach (object obj in treeView.Items)
            {
                var item = treeView.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                if (item != null)
                {
                    item.IsExpanded = true;
                    item.ExpandSubtree();
                }
            }
        }

        public static void CollapseAll(this TreeView treeView)
        {
            foreach (object obj in treeView.Items)
            {
                var item = treeView.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                if (item != null)
                {
                    item.IsExpanded = false;
                    item.CollapseAll();
                }
            }
        }

        public static void CollapseAll(this TreeViewItem treeViewItem)
        {
            foreach (object obj in treeViewItem.Items)
            {
                var item = treeViewItem.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                if (item != null)
                {
                    item.IsExpanded = false;
                    item.CollapseAll();
                }
            }
        }
    }
}
