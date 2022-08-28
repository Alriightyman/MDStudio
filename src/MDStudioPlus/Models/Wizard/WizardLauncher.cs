using MDStudioPlus.Views.Wizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace MDStudioPlus.Models.Wizard
{
    public enum WizardType
    {
        StartPage,
        NewProject,
        ProjectExistingFiles
    }

    public class WizardLauncher : PageFunction<WizardResult>
    {
        private readonly WizardData _wizardData = new WizardData();
        public event WizardReturnEventHandler WizardReturn;
        private WizardType pageType;
        public WizardLauncher(WizardType pageType)
        {
            this.pageType = pageType;
        }

        protected override void Start()
        {
            base.Start();

            // So we remember the WizardCompleted event registration
            KeepAlive = true;

            WizardPageBase page = null;

            // Launch the wizard
            if (pageType == WizardType.StartPage )
            {
                var welcomePage = new WelcomePage(_wizardData);
                page = welcomePage;
            }
            else if (pageType == WizardType.NewProject)
            {
                var newProject = new NewProjectPage(_wizardData);
                page = newProject;
            }
            else if(pageType == WizardType.ProjectExistingFiles)
            {
                var existingFilesPage = new ProjectExistingFilesPage(_wizardData);
                page = existingFilesPage;
            }

            page.Return += wizardPage_Return;
            NavigationService?.Navigate(page);
        }

        public void wizardPage_Return(object sender, ReturnEventArgs<WizardResult> e)
        {
            // Notify client that wizard has completed
            // NOTE: We need this custom event because the Return event cannot be
            // registered by window code - if WizardDialogBox registers an event handler with
            // the WizardLauncher's Return event, the event is not raised.
            WizardReturn?.Invoke(this, new WizardReturnEventArgs(e.Result, _wizardData));
            OnReturn(null);
        }
    }
}
