using MDStudioPlus.FileExplorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MDStudioPlus.ViewModels
{
    internal class ExplorerViewModel : ToolViewModel
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
            var solutionItem = new SolutionItem() { Name = $"Solution '{solution.Name}' ({projectCount} projects)", Path = solution.FullPath };
            solutionItem.Items = new List<Item>();
            solutionItem.OnSelectedItemChanged -= OnSelectedItem;
            solutionItem.OnSelectedItemChanged += OnSelectedItem;
            // go through each project
            foreach (var project in solution.Projects)
            {
                // create a new project tree item
                var projectItem = new ProjectItem() { Name = project.Name, Path = project.FullPath, Parent=solutionItem };
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
                    
                    // determine if the directory exists
                    var directoryItem = projectItem.Items.Where(i => i is DirectoryItem && i.Name == directory).FirstOrDefault() as DirectoryItem;
                    // also, no point if there is no directory
                    if (directoryItem == null && directory != string.Empty)
                    {
                        var dir = directory != string.Empty ? $"{directory}\\" : "";
                        var fullDirpath = $"{currentDir}\\{dir}";
                        directoryItem = new DirectoryItem() { Name = directory, Path = fullDirpath, Parent=projectItem };
                        directoryItem.OnSelectedItemChanged -= OnSelectedItem;
                        directoryItem.OnSelectedItemChanged += OnSelectedItem;
                        projectItem.Items.Add(directoryItem);
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

                // add the project to the solution
                ((List<Item>)projectItem.Items).Sort(
                    delegate (Item p1, Item p2)
                    {
                        bool p1Dir = p1 is DirectoryItem;
                        bool p2Dir = p2 is DirectoryItem;
                        if (p1Dir && !p2Dir)
                        {
                            return -1;
                        }
                        else if (!p1Dir && p2Dir)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    });
                solutionItem.Items.Add(projectItem);
            }

            // clear out the old and bring in the new
            solutionDirectory.Clear();
            solutionDirectory.Add(solutionItem);
           
        }

        #region Events
        private void OnSelectedItem(object sender, SelectedItemEventArgs e)
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
