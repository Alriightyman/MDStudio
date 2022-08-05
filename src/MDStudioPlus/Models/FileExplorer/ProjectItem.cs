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
    public class ProjectItem : DirectoryItem
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

        public ProjectItem(Project project) : base(project)
        {            
            this.Project = project;
        }

        private void OnAddExistingFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ASM (*.asm),(*.s)|*.asm;*.s";
            ofd.Multiselect = true;
            
            // get the project file list
           

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
                DirectoryItem newDirectoryItem = null;
                DirectoryItem parentDirectoryItem = this;
                int projectPathLength = Project.ProjectPath.Length;
                //TODO: test this again
                string relativeFilename = selectedFile.Contains("\\") ? selectedFile.Remove(0, projectPathLength + 1) : selectedFile;
                string fullDirectory = System.IO.Path.GetDirectoryName(relativeFilename);

                if (fullDirectory != String.Empty)
                {
                    var directories = fullDirectory.Split('\\');

                    for (int count = 0; count < directories.Length; count++)
                    {
                        newDirectoryItem = FindDirectoryItem(parentDirectoryItem.Items.Where(di => di is DirectoryItem).Cast<DirectoryItem>().ToList(), directories[count]);

                        // if directory was not found and we have not gone through all directories
                        if (newDirectoryItem == null)
                        {
                            var directory = System.IO.Path.GetDirectoryName(parentDirectoryItem.Path);

                            newDirectoryItem = new DirectoryItem(Project)
                            {
                                Name = directories[count],
                                Path = $"{directory}\\{directories[count]}",
                                Explorer = Explorer,
                                Parent = parentDirectoryItem,
                            };

                            newDirectoryItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
                            newDirectoryItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

                            AddItem<DirectoryItem>(newDirectoryItem, parentDirectoryItem.Items);
                            parentDirectoryItem = newDirectoryItem;
                        }
                        else
                        {
                            parentDirectoryItem = newDirectoryItem;
                        }
                    }
                }

                // now that the directory structure is set, create the file item
                if (!String.IsNullOrEmpty(relativeFilename) && FindFileItem(parentDirectoryItem.Items.ToList(), relativeFilename) == null)
                {
                    // add the file to the project
                    List<string> files = Project.SourceFiles == null ? new List<string>() : Project.SourceFiles.ToList();
                    files.Add(relativeFilename);
                    Project.SourceFiles = files.ToArray();

                    FileItem newFileItem = new FileItem(Project)
                    {
                        // this file path is selected from the file system and
                        // is already a full path
                        Path = selectedFile,
                        Name = System.IO.Path.GetFileName(relativeFilename),
                        Explorer = Explorer,
                        Parent = parentDirectoryItem,
                    };

                    newFileItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
                    newFileItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

                    AddItem<FileItem>(newFileItem, parentDirectoryItem.Items);
                }
            }
        }

        // This will insert an Item into position based on name
        private static void AddItem<T>(Item newItem, ObservableCollection<Item> items)
        {

            if (items.Count == 0)
            {                
                items.Add(newItem);
                return;
            }

            var specificItems = items.Where(i => i is T).ToList();

            if (specificItems.Count == 0)
            {
                if (newItem is DirectoryItem)
                {
                    items.Insert(0, newItem);
                }
                else if (newItem is FileItem)
                {
                    var insertIndex = items.IndexOf(items.LastOrDefault(d => d is DirectoryItem));
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
        private DirectoryItem FindDirectoryItem(List<DirectoryItem> directories, string directoryName)
        {
            foreach(var directory in directories)
            {
                // if found, return
                if (directory.Name.ToLower() == directoryName.ToLower())
                    return directory;
                // not found?  Look through its Items
                List<DirectoryItem> items = directory.Items.Where(item => item is DirectoryItem).Cast<DirectoryItem>().ToList();
                DirectoryItem directoryItem = FindDirectoryItem(items, directoryName);
                
                // if we found it, return
                if (directoryItem != null)
                    return directoryItem;
            }

            // was never found.. just return null instead
            return null;
        }

        // recursively finds the file item, if it exists
        private FileItem FindFileItem(List<Item> items, string filename)
        {
            foreach (var item in items)
            {
                // if found, return
                if (item is FileItem && item.Name.ToLower() == filename.ToLower())
                    return (FileItem)item;

                Item foundFileItem = null;
                // not found?  Look through its Items
                if (item is DirectoryItem dItem)
                {
                    List<Item> newItems = dItem.Items.ToList();
                    foundFileItem = FindFileItem(newItems, filename);
                }
                // if we found it, return
                if (foundFileItem != null && foundFileItem is FileItem fileItem)
                    return fileItem;
            }

            // was never found.. just return null instead
            return null;
        }

    }
}
