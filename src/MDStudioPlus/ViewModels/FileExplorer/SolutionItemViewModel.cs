using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    public class SolutionItemViewModel : DirectoryItemViewModel
    {
        public override string Path
        {
            get;
            set;
        }

        public SolutionItemViewModel() : base(null)
        {

        }
    }
}
