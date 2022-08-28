using MDStudioPlus.Models.Wizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace MDStudioPlus.Views.Wizard
{
    public class WizardPageBase : PageFunction<WizardResult>
    {
        public WizardPageBase(WizardData wizardData)
        {
            DataContext = wizardData;
        }
    }
}
