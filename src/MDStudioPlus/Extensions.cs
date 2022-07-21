using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MDStudioPlus
{
    public static class Extensions
    {

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
