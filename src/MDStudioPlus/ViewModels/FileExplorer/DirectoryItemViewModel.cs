using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MDStudioPlus.FileExplorer.Events;

namespace MDStudioPlus.FileExplorer
{
    public class DirectoryItemViewModel : ItemViewModel, IProjectItemChild
    {
        private RelayCommand addNewFileCommand;

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

        public ICommand AddNewFileCommand
        {
            get
            {
                if(addNewFileCommand == null)
                {
                    addNewFileCommand = new RelayCommand((p) => OnAddNewFile());
                }

                return addNewFileCommand;
            }
        }

        public ObservableCollection<ItemViewModel> Items { get; set; }

        public Project Project { get; protected set; }

        public DirectoryItemViewModel(Project project)
        {
            Items = new ObservableCollection<ItemViewModel>();
            Project = project;
        }

        protected void OnAddNewFile()
        {
            string path = Path;

            if (this is ProjectItemViewModel projItem)
            {
                path = Project.ProjectPath;
            }

            string newFileName = "New File.asm";
            FileItemViewModel fileItem = new FileItemViewModel(Project)
            {
                Name = newFileName,
                Explorer = Explorer,
                //Path = $"{path}\\{newFileName}",
                Parent = this,
            };

            fileItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
            fileItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

            Items.Add(fileItem);

            fileItem.IsRenaming = true;
        }
    }
}
