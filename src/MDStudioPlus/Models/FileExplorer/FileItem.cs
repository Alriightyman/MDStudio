using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    public class FileItem : Item, IProjectItemChild
    {
        public Project Project { get; protected set; }
        public FileItem(Project project)
        {
            Project = project;
        }
    }
}
