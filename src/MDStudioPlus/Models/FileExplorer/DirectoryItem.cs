using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MDStudioPlus.FileExplorer.Events;

namespace MDStudioPlus.FileExplorer
{
    public class DirectoryItem : Item, IProjectItemChild
    {
        protected bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set 
            {
                isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        public ObservableCollection<Item> Items { get; set; }

        public Project Project { get; protected set; }

        public DirectoryItem(Project project)
        {
            Items = new ObservableCollection<Item>();
            Project = project;
        }
    }
}
