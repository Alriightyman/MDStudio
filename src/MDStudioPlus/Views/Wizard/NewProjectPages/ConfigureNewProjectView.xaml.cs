using MDStudioPlus.Models.Wizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MDStudioPlus.Views.Wizard.NewProjectPages
{
    /// <summary>
    /// Interaction logic for ConfigureNewProject.xaml
    /// </summary>
    public partial class ConfigureNewProject : WizardPageBase
    {
        private string newProjectHellowWorldCode = $"./Resources/Source Template/hello_world.s";

        public ConfigureNewProject(WizardData data) : base(data)
        {
            InitializeComponent();
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            Solution solution = new Solution($"{LocationTextbox.Text}\\{this.NameTextbox.Text}.mdsln");
            Project project = new Project($"{LocationTextbox.Text}\\{this.NameTextbox.Text}.mdproj");
            string mainsourceContent = File.ReadAllText(newProjectHellowWorldCode);

            var mainsource = $"{project.ProjectPath}\\main.asm";
            File.WriteAllText(mainsource, mainsourceContent);

            project.MainSourceFile = "main.asm";
            project.Author = this.AuthorTextbox.Text;
            project.Name = this.NameTextbox.Text;          

            project.AssemblerVersion = (AssemblerVersion)this.AssemberVersionCombo.SelectedItem;

            project.PostBuildScript = $"../../../../3rdparty/assemblers/AS/fdp2bin\" \"main.p\" \"main.{project.OutputExtension}\" \"main.h\"";

            string projFile = project.FullPath;
            int length = $"{project.ProjectPath}\\".Length;
            projFile = projFile.Remove(0, length);
            solution.ProjectFiles.Add(projFile);

            project.Save();
            solution.Save();

            project = null;
            solution = null;

            solution = new Solution($"{LocationTextbox.Text}\\{this.NameTextbox.Text}.mdsln");
            //solution.Load();

            ((WizardData)DataContext).Solution = solution;

            OnReturn(new ReturnEventArgs<WizardResult>(WizardResult.SolutionCreated));
        }



        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            // Go to previous wizard page
            NavigationService?.GoBack();
        }

        private void LocationButtonClick(object sender, RoutedEventArgs e)
        {
            //
            using (var obd = new FolderBrowserDialog())
            {
                if (obd.ShowDialog() == DialogResult.OK)
                {

                    string path = obd.SelectedPath;

                    if (path != String.Empty)
                    {
                        LocationTextbox.Text = path;
                    }
                }
            }

            EnableFinishButtonCheck();
        }

        private void EnableFinishButtonCheck()
        {
            bool isReady = NameTextbox.Text != String.Empty
                        && LocationTextbox.Text != String.Empty
                          && (AssemberVersionCombo.SelectedItem != null && (AssemblerVersion)AssemberVersionCombo.SelectedItem != AssemblerVersion.None);

            FinishButtion.IsEnabled = isReady;
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

        private void NameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableFinishButtonCheck();
        }

        private void LocationTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableFinishButtonCheck();
        }

        private void AssemberVersionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableFinishButtonCheck();
        }
    }
}
