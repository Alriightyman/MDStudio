using MDStudioPlus.FileExplorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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

        #region methods
        private void SetFiles()
        {
            var currentDir = solution.SolutionPath;
            int projectCount = solution.Projects.Count;
            // set up name for solution
            var solutionItem = new SolutionItem() { Name = $"Solution '{solution.Name}' ({projectCount} projects)", Path = solution.FullPath, Explorer=this };
            solutionItem.Items = new ObservableCollection<Item>();
            solutionItem.OnSelectedItemChanged -= OnSelectedItem;
            solutionItem.OnSelectedItemChanged += OnSelectedItem;
            // go through each project
            foreach (var project in solution.Projects)
            {
                // create a new project tree item
                var projectItem = new ProjectItem(project) { Name = project.Name, Path = project.FullPath, Parent=solutionItem, Explorer = this };
                projectItem.OnSelectedItemChanged -= OnSelectedItem;
                projectItem.OnSelectedItemChanged += OnSelectedItem;
                // get all of the files in this project and sort
                List<string> files = (List<string>)project.AllFiles();
                files.Sort();


                foreach(var file in files)
                {
                    // get the directory the file lives under
                    var directory = Path.GetDirectoryName(file);
                    // get the filename of the file
                    var filename = Path.GetFileName(file);

                    // only bother if the file doesn't exist
                    var existingFileItem = projectItem.Items.Where(i => i is FileItem && i.Name.ToLower() == filename.ToLower()).FirstOrDefault();
                    if (existingFileItem == null)
                    {
                        foreach (var item in projectItem.Items)
                        {
                            if (item is DirectoryItem directoryItem)
                            {
                                existingFileItem = directoryItem.Items.Where(i => i is FileItem && i.Name.ToLower() == filename.ToLower()).FirstOrDefault() as FileItem;
                                if (existingFileItem != null)
                                    break;
                            }
                        }
                    }

                    if (existingFileItem == null)
                    {

                        DirectoryItem directoryItem = null;
                        // TODO: Check multiple Directories..
                        string[] directories = directory.Split('\\');
                        DirectoryItem possibleParent = null;

                        foreach (var dir in directories)
                        {
                            // determine if the directory exists
                            directoryItem = projectItem.Items.Where(i => i is DirectoryItem && i.Name.ToLower() == dir.ToLower()).FirstOrDefault() as DirectoryItem;

                            // 
                            if (directoryItem == null)
                            {
                                foreach(var item in projectItem.Items)
                                {
                                    if (item is DirectoryItem dirItem)
                                    {
                                        directoryItem = FindDirectory(dirItem, dir);
                                    }
                                }
                            }

                            // also, no point if there is no directory
                            if (directoryItem == null && directory != string.Empty)
                            {
                                var dirAdjustment = dir != string.Empty ? $"{dir}\\" : "";
                                var fullDirpath = $"{currentDir}\\{dirAdjustment}";
                                directoryItem = new DirectoryItem() { Name = dir.ToLower(), Path = fullDirpath.ToLower(), Parent = (possibleParent ?? projectItem), Explorer = this };
                                directoryItem.OnSelectedItemChanged -= OnSelectedItem;
                                directoryItem.OnSelectedItemChanged += OnSelectedItem;
                                if (possibleParent != null)
                                {
                                    possibleParent.Items.Add(directoryItem);
                                }
                                else
                                {
                                    projectItem.Items.Add(directoryItem);
                                }                                
                            }

                            possibleParent = directoryItem as DirectoryItem;
                        }

                        // create the file and add it to the directory (if it exists)
                        // otherwise, add it to the project
                        var fileItem = new FileItem() { Name = filename, Path = $"{project.ProjectPath}\\{file}" };
                        fileItem.OnSelectedItemChanged -= OnSelectedItem;
                        fileItem.OnSelectedItemChanged += OnSelectedItem;
                        if (directory != string.Empty)
                        {
                            fileItem.Parent = directoryItem;
                            directoryItem.Items.Add(fileItem);
                        }
                        else
                        {
                            fileItem.Parent = projectItem;
                            projectItem.Items.Add(fileItem);
                        }                        
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
