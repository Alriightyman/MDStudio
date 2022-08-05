using MDStudioPlus.FileExplorer;
using MDStudioPlus.FileExplorer.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace MDStudioPlus.ViewModels
{
    public class ExplorerViewModel : ToolViewModel
    {
        #region Event Handlers
        public event SelectedItemEventHandler OnSelectedItemChanged;
        #endregion
        
        #region fields
        public const string ToolContentId = "FileStatsTool";
        private DateTime lastModified;
        private long fileSize;
        private string fileName;
        private string filePath;

        private Solution solution;
        private RelayCommand expandCommand;
        #endregion fields

        private ObservableCollection<Item> solutionDirectory = new ObservableCollection<Item>();
        
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public ExplorerViewModel()
            : base("Solution Explorer")
        {
            Workspace.Instance.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
            ContentId = ToolContentId;
        }
        #endregion constructors

        #region Properties
        public Item SelectedItem { get; set; }
        public ObservableCollection<Item> DirectoryItems
        {
            get => solutionDirectory;
            set
            {
                solutionDirectory = value;
                RaisePropertyChanged(nameof(DirectoryItems));
            }
        }

        public Solution Solution
        {
            get => solution;
            set
            {
                if (value != null && solution != value)
                {
                    solution = value;
                    FileName = solution.Name;
                    FilePath = solution.FullPath;
                    solution.Load();

                    SetFiles();
                }
                else if(value == null)
                {
                    solution = value;
                    DirectoryItems.Clear();
                }
                
                RaisePropertyChanged(nameof(Solution));
            }
        }

        public long FileSize
        {
            get => fileSize;
            protected set
            {
                if (fileSize != value)
                {
                    fileSize = value;
                    RaisePropertyChanged(nameof(FileSize));
                }
            }
        }

        public DateTime LastModified
        {
            get => lastModified;
            protected set
            {
                if (lastModified != value)
                {
                    lastModified = value;
                    RaisePropertyChanged(nameof(LastModified));
                }
            }
        }

        public string FileName
        {
            get => fileName;
            protected set
            {
                if (fileName != value)
                {
                    fileName = value;
                    RaisePropertyChanged(nameof(FileName));
                }
            }
        }

        public string FilePath
        {
            get => filePath;
            protected set
            {
                if (filePath != value)
                {
                    filePath = value;
                    RaisePropertyChanged(nameof(FilePath));
                }
            }
        }

        #endregion Properties

        #region Commands
        public ICommand ExpandCommand
        {
            get
            {
                if(expandCommand == null)
                {
                    expandCommand = new RelayCommand((p) => OnExpand());
                }

                return expandCommand;
            }
        }
        #endregion

        #region methods

        private void OnExpand()
        {
            ((DirectoryItem)this.DirectoryItems.FirstOrDefault()).IsExpanded = true;
        }

        private void SetFiles()
        {
            var currentDir = solution.SolutionPath;
            int projectCount = solution.Projects.Count;
            // set up name for solution
            var solutionItem = new SolutionItem() { Name = $"Solution '{solution.Name}' ({projectCount} projects)", Path = solution.FullPath, Explorer=this };
            solutionItem.IsExpanded = true;

            solutionItem.Items = new ObservableCollection<Item>();
            solutionItem.OnSelectedItemChanged -= OnSelectedItem;
            solutionItem.OnSelectedItemChanged += OnSelectedItem;
            // go through each project
            foreach (var project in solution.Projects)
            {
                // create a new project tree item
                var projectItem = new ProjectItem(project) { Name = project.Name, Path = project.FullPath, Parent=solutionItem, Explorer = this };
                projectItem.IsExpanded = true;

                projectItem.OnSelectedItemChanged -= OnSelectedItem;
                projectItem.OnSelectedItemChanged += OnSelectedItem;
                // get all of the files in this project and sort
                List<string> files = (List<string>)project.AllFiles();
                files.Sort();

                foreach (string selectedFile in project.AllFiles())
                {
                    DirectoryItem newDirectoryItem = null;
                    DirectoryItem parentDirectoryItem = projectItem;
                    int projectPathLength = project.ProjectPath.Length;
                    string relativeFilename =  selectedFile;
                    string fullDirectory = System.IO.Path.GetDirectoryName(relativeFilename);

                    if (fullDirectory != String.Empty)
                    {
                        var directories = fullDirectory.Split('\\');


                        for (int count = 0; count < directories.Length; count++)
                        {
                            newDirectoryItem = FindDirectory(parentDirectoryItem.Items.Where(di => di is DirectoryItem).Cast<DirectoryItem>().ToList(), directories[count]);

                            // if directory was not found and we have not gone through all directories
                            if (newDirectoryItem == null)
                            {
                                var directory = System.IO.Path.GetDirectoryName(parentDirectoryItem.Path);
                                newDirectoryItem = new DirectoryItem(project)
                                {
                                    Name = directories[count],
                                    Path = $"{directory}\\{directories[count]}",
                                    Explorer = this,
                                    Parent = parentDirectoryItem,
                                };

                                newDirectoryItem.OnSelectedItemChanged -= OnSelectedItem;
                                newDirectoryItem.OnSelectedItemChanged += OnSelectedItem;

                                parentDirectoryItem.Items.Add(newDirectoryItem);
                                parentDirectoryItem = newDirectoryItem;
                            }
                            else
                            {
                                parentDirectoryItem = newDirectoryItem;
                            }
                        }
                    }

                    // now that the directory structure is set, create the file item
                    if (!String.IsNullOrEmpty(relativeFilename))
                    {
                        FileItem newFileItem = new FileItem(project)
                        {
                            // since the project holds relative paths, we need to 
                            // create a full path
                            Path = $"{project.ProjectPath}\\{selectedFile}",
                            Name = System.IO.Path.GetFileName(relativeFilename),
                            Explorer = this,
                            Parent = parentDirectoryItem,
                        };

                        newFileItem.OnSelectedItemChanged -= OnSelectedItem;
                        newFileItem.OnSelectedItemChanged += OnSelectedItem;

                        parentDirectoryItem.Items.Add(newFileItem);
                    }
                }

                // add the project to the solution
                List<Item> projectList = Sort(projectItem.Items.ToList());
                projectItem.Items = new ObservableCollection<Item>(projectList);
                solutionItem.Items.Add(projectItem);
            }

            // clear out the old and bring in the new
            solutionDirectory.Clear();
            solutionDirectory.Add(solutionItem);
           
        }

        private DirectoryItem FindDirectory(List<DirectoryItem> directories, string directoryName)
        {
            foreach (var directory in directories)
            {
                // if found, return
                if (directory.Name.ToLower() == directoryName.ToLower())
                    return directory;
                // not found?  Look through its Items
                List<DirectoryItem> items = directory.Items.Where(item => item is DirectoryItem).Cast<DirectoryItem>().ToList();
                DirectoryItem directoryItem = FindDirectory(items, directoryName);

                // if we found it, return
                if (directoryItem != null)
                    return directoryItem;
            }

            // was never found.. just return null instead
            return null;
        }

        // sort the directory items in the hierarchy
        private List<Item> Sort(List<Item> listItems)
        {
            var sortedList = listItems.OrderByDescending(x => x.GetType() == typeof(DirectoryItem)).ThenBy(z => z.Name).ToList();

            foreach(var item in sortedList)
            {
                if(item is DirectoryItem directoryItem)
                {
                    directoryItem.Items = new ObservableCollection<Item>(Sort(directoryItem.Items.ToList()));
                }
            }

            return sortedList;
        }

        private DirectoryItem FindDirectory(DirectoryItem directoryItem, string name)
        {
            if (directoryItem == null)
                return null;

            if(directoryItem.Name == name)
                return directoryItem;

            DirectoryItem foundDirectoryItem = null;

            foreach (Item item in directoryItem.Items)
            {
                if (item is DirectoryItem dirItem)
                {
                    foundDirectoryItem = FindDirectory(dirItem, name);

                    if(foundDirectoryItem != null)
                        return foundDirectoryItem;
                }
            }

            return null;
        }

        #region Events
        public void OnSelectedItem(object sender, SelectedItemEventArgs e)
        {
            SelectedItem = e.SelectedItem;

            OnSelectedItemChanged?.Invoke(this, e);
        }

        private void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            if (Workspace.Instance.ActiveDocument != null &&
                Workspace.Instance.ActiveDocument.FilePath != null &&
                File.Exists(Workspace.Instance.ActiveDocument.FilePath))
            {
                var fi = new FileInfo(Workspace.Instance.ActiveDocument.FilePath);
                FileSize = fi.Length;
                LastModified = fi.LastWriteTime;
                FileName = fi.Name;
                FilePath = fi.Directory.FullName;
            }
            else
            {
                FileSize = 0;
                LastModified = DateTime.MinValue;
                FileName = string.Empty;
                FilePath = string.Empty;
            }
        }
        #endregion Events
        
        #endregion methods

    }
}
