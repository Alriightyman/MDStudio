using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MDStudioPlus.ViewModels
{
    public class ProjectPropertiesViewModel : ViewModelBase
    {
        private Project project;
        private RelayCommand addItemCommand;
        private RelayCommand<ObservableCollection<object>> removeItemCommand;
        public string Name
        {
            get { return project.Name; }
            set
            {
                if (project?.Name != value)
                {
                    project.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public string Author
        {
            get => project.Author;
            set
            {
                if (project?.Author != value)
                {
                    project.Author = value;
                    RaisePropertyChanged(nameof(Author));
                }
            }
        }

        public string OutputFileName
        {
            get => project.OutputFileName;
            set
            {
                if(project?.OutputFileName != value)
                {
                    project.OutputFileName = value;
                    RaisePropertyChanged(nameof(OutputFileName));
                }
            }
        }

        public string OutputExtension
        {
            get => project.OutputExtension;
            set
            {
                if(project.OutputExtension != value)
                {
                    project.OutputExtension = value;
                    RaisePropertyChanged(nameof(OutputExtension));
                }
            }
        }

        public IEnumerable<AssemblerVersion> AssemberVersions
        {
            get => Enum.GetValues(typeof(AssemblerVersion)).Cast<AssemblerVersion>();
        }

        public AssemblerVersion AssemblerVersion
        {
            get => project.AssemblerVersion;
            set
            {
                if (project?.AssemblerVersion != value)
                {
                    project.AssemblerVersion = value;
                    RaisePropertyChanged(nameof(AssemblerVersion));
                }
            }
        }

        public string PreBuildScript
        {
            get => project.PreBuildScript;
            set
            {
                if (project?.PreBuildScript != value)
                {
                    project.PreBuildScript = value;
                    RaisePropertyChanged(nameof(PreBuildScript));
                }
            }
        }

        public string PostBuildScript
        {
            get => project.PostBuildScript;
            set
            {
                if (project?.PostBuildScript != value)
                {
                    project.PostBuildScript = value;
                    RaisePropertyChanged(nameof(PostBuildScript));
                }
            }
        }

        public string AdditionalArguments
        {
            get => project.AdditionalArguments;
            set
            {
                if (project?.AdditionalArguments != value)
                {
                    project.AdditionalArguments = value;
                    RaisePropertyChanged(nameof(AdditionalArguments));
                }
            }
        }

        public List<string> FilesToExclude
        {
            get => project.FilesToExclude.ToList();
            set
            {
                if(project.FilesToExclude != value.ToArray())
                {
                    project.FilesToExclude = value.ToArray();
                    RaisePropertyChanged(nameof(FilesToExclude));
                }
            }
        }

        public ICommand AddItemCommand
        {
            get
            {
                if(addItemCommand == null)
                {
                    addItemCommand = new RelayCommand((p) => OnAddItem());
                }

                return addItemCommand;
            }
        }

        public ICommand RemoveItemCommand
        {
            get
            {
                if(removeItemCommand == null)
                {
                    removeItemCommand = new RelayCommand<ObservableCollection<object>>((p) => OnRemoveItem(p));
                }

                return removeItemCommand;
            }
        }

        private void OnAddItem()
        {
            // custom dialog box that list all of the items
            var files = FilesToExclude;
            files.Add(project.Name);
            FilesToExclude = files;
            
        }

        private void OnRemoveItem(ObservableCollection<object> param)
        {
            var files = FilesToExclude;
            foreach(string file in param)
            {
                files.Remove(file);
            }
            FilesToExclude = files;
        }

        public ProjectPropertiesViewModel(Project project)
        {
            this.project = project;
        }
    }
}
