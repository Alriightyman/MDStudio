using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MDStudioPlus.FileExplorer
{
    public class ProjectItemViewModel : DirectoryItemViewModel
    {
        private RelayCommand addExistingFileCommand;

        public ICommand AddExistingFileCommand
        {
            get
            {
                if(addExistingFileCommand == null)
                {
                    addExistingFileCommand = new RelayCommand((p) => OnAddExistingFile());
                }    
                return addExistingFileCommand;
            }
        }


        public override string Path
        {
            get;
            set;
        }

        public ProjectItemViewModel(Project project) : base(project)
        {            
            this.Project = project;
        }

        private void OnAddExistingFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ASM (*.asm),(*.s)|*.asm;*.s";
            ofd.Multiselect = true;
            
            if( ofd.ShowDialog() == true)
            {

                AddExsitingFiles(ofd.FileNames);

                Project.Save();
            }            
        }

        // adds the an existing file into the project
        // this will add any folders as well
        private void AddExsitingFiles(string[] selectedFiles)
        {
            foreach(string selectedFile in selectedFiles)
            {
                DirectoryItemViewModel newDirectoryItem = null;
                DirectoryItemViewModel parentDirectoryItem = this;
                int projectPathLength = Project.ProjectPath.Length;
                //TODO: test this again
                string relativeFilename = selectedFile.Contains("\\") ? selectedFile.Remove(0, projectPathLength + 1) : selectedFile;
                string fullDirectory = System.IO.Path.GetDirectoryName(relativeFilename);
                string previousDirectoryName = string.Empty;

                if (fullDirectory != String.Empty)
                {
                    var directories = fullDirectory.Split('\\');

                    for (int count = 0; count < directories.Length; count++)
                    {
                        newDirectoryItem = FindDirectoryItem(parentDirectoryItem.Items.Where(di => di is DirectoryItemViewModel).Cast<DirectoryItemViewModel>().ToList(), directories[count], count != 0 ? directories[count-1] : String.Empty);

                        // if directory was not found and we have not gone through all directories
                        if (newDirectoryItem == null)
                        {
                            var directory = System.IO.Path.GetDirectoryName(parentDirectoryItem.Path);

                            newDirectoryItem = new DirectoryItemViewModel(Project)
                            {
                                Name = directories[count],
                                //Path = $"{directory}\\{directories[count]}",
                                Explorer = Explorer,
                                Parent = parentDirectoryItem,
                            };

                            newDirectoryItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
                            newDirectoryItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

                            AddItem<DirectoryItemViewModel>(newDirectoryItem, parentDirectoryItem.Items);
                            parentDirectoryItem = newDirectoryItem;
                        }
                        else
                        {
                            parentDirectoryItem = newDirectoryItem;
                        }
                    }
                }

                // now that the directory structure is set, create the file item
                if (!relativeFilename.IsNullOrEmpty() && FindFileItem(parentDirectoryItem.Items.ToList(), relativeFilename) == null)
                {
                    // add the file to the project
                    List<string> files = Project.SourceFiles == null ? new List<string>() : Project.SourceFiles.ToList();
                    files.Add(relativeFilename);
                    Project.SourceFiles = files.ToArray();

                    FileItemViewModel newFileItem = new FileItemViewModel(Project)
                    {
                        // this file path is selected from the file system and
                        // is already a full path
                        //Path = selectedFile,
                        Name = System.IO.Path.GetFileName(relativeFilename),
                        Explorer = Explorer,
                        Parent = parentDirectoryItem,
                    };

                    newFileItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
                    newFileItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

                    AddItem<FileItemViewModel>(newFileItem, parentDirectoryItem.Items);
                }
            }
        }

        // This will insert an Item into position based on name
        private static void AddItem<T>(ItemViewModel newItem, ObservableCollection<ItemViewModel> items)
        {

            if (items.Count == 0)
            {                
                items.Add(newItem);
                return;
            }

            var specificItems = items.Where(i => i is T).ToList();

            if (specificItems.Count == 0)
            {
                if (newItem is DirectoryItemViewModel)
                {
                    items.Insert(0, newItem);
                }
                else if (newItem is FileItemViewModel)
                {
                    var insertIndex = items.IndexOf(items.LastOrDefault(d => d is DirectoryItemViewModel));
                    if (insertIndex != -1)
                    {
                        items.Insert(insertIndex, newItem);
                    }
                    else
                    {
                        items.Add(newItem);
                    }
                }
                return;
            }

            int index = 0;
            for (; index < specificItems.Count; index++)
            {
                int pos = newItem.Name.ToLower().CompareTo(specificItems[index].Name.ToLower());

                // if same or before
                if (pos == -1)
                {
                    int realIndex = items.IndexOf(specificItems[index]);
                    items.Insert(realIndex, newItem);
                    return;
                }                    
            }

            // just add at the end...
            items.Add(newItem);
        }

        // recursively finds the directory item, if it exists
        private DirectoryItemViewModel FindDirectoryItem(List<DirectoryItemViewModel> directories, string directoryName, string previousDirectory)
        {
            foreach(var directory in directories)
            {
                // if found, return
                if (directory.Name.ToLower() == directoryName.ToLower())
                    return directory;
                // not found?  Look through its Items
                List<DirectoryItemViewModel> items = directory.Items.Where(item => item is DirectoryItemViewModel).Cast<DirectoryItemViewModel>().ToList();
                DirectoryItemViewModel directoryItem = FindDirectoryItem(items, directoryName, previousDirectory);
                
                // if we found it, varify that its in the correct place
                if (directoryItem != null &&
                    ( (directoryItem.Parent != null && directoryItem.Parent.Name == previousDirectory) ||
                    (previousDirectory != String.Empty && directoryItem.Parent is IProjectItemChild)))
                    return directoryItem;
            }

            // was never found.. just return null instead
            return null;
        }

        // recursively finds the file item, if it exists
        private FileItemViewModel FindFileItem(List<ItemViewModel> items, string filename)
        {
            foreach (var item in items)
            {
                // if found, return
                if (item is FileItemViewModel && item.Name.ToLower() == filename.ToLower())
                    return (FileItemViewModel)item;

                ItemViewModel foundFileItem = null;
                // not found?  Look through its Items
                if (item is DirectoryItemViewModel dItem)
                {
                    List<ItemViewModel> newItems = dItem.Items.ToList();
                    foundFileItem = FindFileItem(newItems, filename);
                }
                // if we found it, return
                if (foundFileItem != null && foundFileItem is FileItemViewModel fileItem)
                    return fileItem;
            }

            // was never found.. just return null instead
            return null;
        }

    }
}
