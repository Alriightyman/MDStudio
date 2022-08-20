using MDStudioPlus.Models.Wizard;
using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MDStudioPlus.Views.Wizard
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : WizardPageBase
    {
        Config Config
        {
            get => Workspace.Instance.ConfigViewModel.Config;
            set => Workspace.Instance.ConfigViewModel.Config = value;
        }

        public WelcomePage(WizardData wizardData) : base(wizardData)
        {
            InitializeComponent();

            RecentItemsListView.ItemsSource = Config.RecentProjects;
        }

        private void CreateProjectFromExistingFiles_Click(object sender, RoutedEventArgs e)
        {
            // Go to next wizard page
            var wizardPage2 = new ProjectExistingFilesPage((WizardData)DataContext);
            wizardPage2.Return += wizardPage_Return;
            NavigationService?.Navigate(wizardPage2);
        }

        private void CreateNewProject_Click(object sender, RoutedEventArgs e)
        {
            // Go to next wizard page
            var wizardPage2 = new NewProjectPage((WizardData)DataContext);
            wizardPage2.Return += wizardPage_Return;
            NavigationService?.Navigate(wizardPage2);
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            // Cancel the wizard and don't return any data
            OnReturn(new ReturnEventArgs<WizardResult>(WizardResult.Canceled));
        }

        public void wizardPage_Return(object sender, ReturnEventArgs<WizardResult> e)
        {
            // If returning, wizard was completed (finished or canceled),
            // so continue returning to calling page
            OnReturn(e);
        }

        private void RecentItemsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (RecentItemsListView.SelectedItem != null)
                {
                    var data = (WizardData)DataContext;
                    data.RecentProjectPath = RecentItemsListView.SelectedItem.ToString();
                    OnReturn(new ReturnEventArgs<WizardResult>(WizardResult.RecentProjectSelected));
                }
            }
        }
    }
}
