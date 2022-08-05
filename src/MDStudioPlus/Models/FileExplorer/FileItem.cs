using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    public class FileItem : Item, IProjectItemChild
    {
        public Project Project { get; set; }
        public FileItem(Project project)
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
            
            if(Project.MainSourceFile == relativePath)
            {
                // rename it then
                Project.MainSourceFile = newPath.Remove(0,projectPathCount);
            }

            Path = newPath;
            Project.Save();

            // this will perform a rename in the file system
            File.Move(fullPath, newPath);

            // do this last
            base.OnLostFocus(param);            
        }
    }
}
