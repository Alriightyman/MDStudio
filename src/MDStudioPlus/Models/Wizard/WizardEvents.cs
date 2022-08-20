using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Models.Wizard
{
    public enum WizardResult
    {
        SolutionCreated,
        ProjectCreated,
        RecentProjectSelected,
        Canceled
    }

    public delegate void WizardReturnEventHandler(object sender, WizardReturnEventArgs e);

    public class WizardReturnEventArgs
    {
        public WizardReturnEventArgs(WizardResult result, object data)
        {
            Result = result;
            Data = data;
        }

        public WizardResult Result { get; }
        public object Data { get; }
    }
}
