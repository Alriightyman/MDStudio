using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    public class FileItemViewModel : ItemViewModel, IProjectItemChild
    {
        public Project Project { get; set; }
        public FileItemViewModel(Project project)
        {
            Project = project;
        }

        public bool IsExpanded
        {
            get => false;
            set { }
        }

        protected override void OnLostFocus(object param)
        {
            string newName = (string)param;            

            string fullPath = Path;
            string newPath = Path.Replace(oldName, newName);

            // check if this is from the main source file first...
            int projectPathCount = Project.ProjectPath.Length + 1;
            var relativePath = fullPath.Remove(0, projectPathCount);
            var newRelativePath = newPath.Remove(0, projectPathCount);

            if (Project.MainSourceFile == relativePath)
            {
                // rename it then
                Project.MainSourceFile = newRelativePath;
            }
            else // it somewhere in sourcefiles
            {
                bool fileFound = false;
                for(int index = 0; index < Project.SourceFiles.Length; index++)
                {
                    var sourceFile = Project.SourceFiles[index];
                    if(sourceFile == relativePath)
                    {
                        Project.SourceFiles[index] = newRelativePath;
                        fileFound = true;
                        break;
                    }
                }

                // if it doesn't exist in the project
                // then we should add it because its
                // probably a new file
                if(!fileFound)
                {
                    var files = Project.SourceFiles.ToList();
                    files.Add(newRelativePath);
                    Project.SourceFiles = files.ToArray();
                }
            }

            File.WriteAllText(newPath, String.Empty);
            
            //Path = newPath;
            Project.Save();

            // this will perform a rename in the file system
            if (File.Exists(fullPath))
            {
                File.Move(fullPath, newPath);
            }

            // do this last
            base.OnLostFocus(param);            
        }
    }
}
