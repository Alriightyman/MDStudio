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
        private Project project;

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

        public ProjectItem(Project project) : base()
        {            
            this.project = project;
        }

        private void OnAddExistingFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ASM (*.asm),(*.s)|*.asm;*.s";
            ofd.Multiselect = true;

            if( ofd.ShowDialog() == true)
            {
                // get the project file list
                var files = project.SourceFiles.ToList();

                // go through each selected file
                foreach (var selectedFile in ofd.FileNames)
                {
                    // get the project's directory
                    var currentDir = project.ProjectPath;
                    var file = selectedFile;

                    // remove project path
                    file = file.Replace('/', '\\');
                    var length = project.ProjectPath.Length;
                    file = file.Remove(0, length+1);

                    // get the directory the file lives under
                    var directory = System.IO.Path.GetDirectoryName(file);
                    // get the filename of the file
                    var filename = System.IO.Path.GetFileName(file);

                    // check if the file exists
                    var existingFileItem = Items.Where(i => i is FileItem && i.Name.ToLower() == filename.ToLower()).FirstOrDefault();

                    // if we didn't find it, it could be in another directory
                    if (existingFileItem == null)
                    {
                        foreach(var item in Items)
                        {
                            if(item is DirectoryItem directoryItem)
                            {
                                existingFileItem = directoryItem.Items.Where(i => i is FileItem && i.Name.ToLower() == filename.ToLower()).FirstOrDefault() as FileItem;
                                if (existingFileItem != null)
                                    break;
                            }
                        }                        
                    }

                    // only bother if the file doesn't exist
                    if (existingFileItem == null)
                    {
                        // add file to list
                        files.Add(file);

                        DirectoryItem directoryItem = null;
                        DirectoryItem possibleParent = null;
                        
                        string[] directories = directory.Split('\\');

                        // walk through and add any non-existant directories
                        foreach (var dir in directories)
                        {
                            // find if the directory exists under the project directory
                            directoryItem = Items.Where(i => i is DirectoryItem && i.Name.ToLower() == dir.ToLower()).FirstOrDefault() as DirectoryItem;

                            // if not found, might be under another directory
                            if (directoryItem == null)
                            {
                                var directoryItems = Items.Where(i => i is DirectoryItem);
                                foreach (DirectoryItem item in directoryItems)
                                {   
                                    // search through this directory
                                    directoryItem = FindDirectory(item, dir);
                                    if (directoryItem != null)
                                        break;
                                }
                            }

                            // if the directory doesn't exist, add it
                            if (directoryItem == null && directory != string.Empty)
                            {
                                var dirAdjustment = dir != string.Empty ? $"{dir}\\" : "";
                                var fullDirpath = $"{currentDir}\\{dirAdjustment}";
                                directoryItem = new DirectoryItem() { Name = dir.ToLower(), Path = fullDirpath.ToLower(), Parent = (possibleParent ?? this), Explorer = Explorer };
                                directoryItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
                                directoryItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

                                if (possibleParent != null)
                                {
                                    AddItem(directoryItem, possibleParent.Items);
                                }
                                else
                                {
                                    AddItem(directoryItem, Items);
                                }
                            }

                            // keep this as it could be the parent directory
                            possibleParent = directoryItem as DirectoryItem;
                        }

                        // create the file and add it to the directory (if it exists)
                        // otherwise, add it to the project                    
                        var fileItem = new FileItem() { Name = filename.ToLower(), Path = $"{project.ProjectPath}\\{file}".ToLower(), Explorer = Explorer };
                        fileItem.OnSelectedItemChanged -= Explorer.OnSelectedItem;
                        fileItem.OnSelectedItemChanged += Explorer.OnSelectedItem;

                        if (directory != string.Empty)
                        {
                            fileItem.Parent = directoryItem;                        
                            AddItem(fileItem, directoryItem.Items);
                        }
                        else
                        {
                            fileItem.Parent = this;
                            AddItem(fileItem, Items);
                        }
                    }
                }

                // udpate the project source files and save
                project.SourceFiles = files.ToArray();
                project.Save();
            }            
        }

        // This will insert an Item into position based on name
        private static void AddItem(Item newItem, ObservableCollection<Item> items)
        {
            if (items.Count == 0)
            {
                items.Add(newItem);
                return;
            }

            int index = 0;
            for(; index < items.Count; index++)
            {
                int pos = newItem.Name.ToLower().CompareTo(items[index].Name.ToLower());
                
                // if same or before
                if (pos == 0 || pos == -1)
                {
                    items.Insert(index, newItem);
                    break;
                }                
            }
        }

        private DirectoryItem FindDirectory(DirectoryItem directoryItem, string name)
        {
            if (directoryItem == null)
                return null;

            if (directoryItem.Name.ToLower() == name.ToLower())
                return directoryItem;

            DirectoryItem foundDirectoryItem = null;
            var directoryItems = directoryItem.Items.Where(x => x is DirectoryItem);
            foreach (DirectoryItem item in directoryItems)
            {
                    foundDirectoryItem = FindDirectory(item, name);

                    if (foundDirectoryItem != null)
                        return foundDirectoryItem;
                
            }

            return null;
        }
    }
}
