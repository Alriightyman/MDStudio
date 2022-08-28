﻿using MDStudioPlus.Models.Wizard;
using Microsoft.Win32;
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

namespace MDStudioPlus.Views.Wizard.ExistingFilesPages
{
    /// <summary>
    /// Interaction logic for ConfigureProjectExistingView.xaml
    /// </summary>
    public partial class ConfigureProjectExistingView : WizardPageBase
    {
        public ConfigureProjectExistingView(WizardData data) : base(data)
        {
            InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            // Go to previous wizard page
            NavigationService?.GoBack();
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            Solution solution = new Solution($"{LocationTextbox.Text}\\{this.NameTextbox.Text}.mdsln");
            Project project = new Project($"{LocationTextbox.Text}\\{this.NameTextbox.Text}.mdproj");
            int length = $"{project.ProjectPath}\\".Length;
            string mainsource = this.MainSourceTextbox.Text.Remove(0, length);
            project.MainSourceFile = mainsource;
            project.Author = this.AuthorTextbox.Text;
            project.Name = this.NameTextbox.Text;
            project.PostBuildScript = this.PostbuildTextbox.Text;
            project.PreBuildScript = this.PrebuildTextbox.Text;

            List<string> files = new List<string>();
            foreach (var item in SourceFilesListbox.Items)
            {
                string file = (string)item;
                length = $"{project.ProjectPath}\\".Length;
                file = file.Remove(0, length);
                files.Add(file);
            }

            project.SourceFiles = files.ToArray();


            files = new List<string>();
            foreach (var item in ExcludedFilesListbox.Items)
            {
                string file = (string)item;
                length = $"{project.ProjectPath}\\".Length;
                file = file.Remove(0, length);
                files.Add(file);
            }

            project.FilesToExclude = files.ToArray();

            project.OutputExtension = this.BinaryExtensionTextbox.Text;
            project.OutputFileName = this.BinaryNameTextbox.Text;

            project.AssemblerVersion = (AssemblerVersion)this.AssemberVersionCombo.SelectedItem;
            project.AdditionalArguments = this.ArgsTextbox.Text;

            string projFile = project.FullPath;
            length = $"{project.ProjectPath}\\".Length;
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

        private void MainSourceButtonClick(object sender, RoutedEventArgs e)
        {
            //
            string sourceFile = OpenFile(false).FirstOrDefault() ?? String.Empty;

            if (sourceFile != String.Empty)
            {
                MainSourceTextbox.Text = sourceFile;
            }
            EnableFinishButtonCheck();
        }
        
        private void AddSourceFile_Click(object sender, RoutedEventArgs e)
        {
            //
            string[] sourceFile = OpenFile(true);

            foreach(string file in sourceFile)
            {
                SourceFilesListbox.Items.Add(file);
            }
        }

        private void RemoveSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = SourceFilesListbox.SelectedItem;
            if (selected != null)
            {
                SourceFilesListbox.Items.Remove(selected);
            }
        }

        private void AddExcludedFile_Click(object sender, RoutedEventArgs e)
        {
            string[] sourceFile = OpenFile(true);

            foreach (string file in sourceFile)
            {
                ExcludedFilesListbox.Items.Add(file);
            }
        }

        private void RemoveExcludedFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ExcludedFilesListbox.SelectedItem;
            if (selected != null)
            {
                ExcludedFilesListbox.Items.Remove(selected);
            }
        }

        private string[] OpenFile(bool isMutlifile)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Assembly Files (*.s,*.asm)|*.s;*.asm|All Files (*.*)|*.*";
            dlg.Multiselect = isMutlifile;
            if (dlg.ShowDialog() == true)
            {
                if(isMutlifile)
                {
                    return dlg.FileNames;
                }
                else
                {
                    return new string[] {dlg.FileName};
                }
            }

            return null;
        }

        private void EnableFinishButtonCheck()
        {
            bool isReady = NameTextbox.Text != String.Empty
                        && LocationTextbox.Text != String.Empty
                          && MainSourceTextbox.Text != String.Empty
                          && (AssemblerVersion)AssemberVersionCombo.SelectedItem != AssemblerVersion.None;

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

        private void MainSourceTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableFinishButtonCheck();
        }

        private void BinaryNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.BinaryNameTextbox.Text = BinaryNameTextbox.Text.Replace(" ", "_");
        }
    }
}
