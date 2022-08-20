using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Models.Wizard
{
    public class WizardData
    {
        public WizardResult Result { get; set; }

        public Solution Solution { get; set; }

        public Project Project { get; set; }      
        
        public string RecentProjectPath { get; set; }
    }
}
