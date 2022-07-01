using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDStudio
{
    public partial class NewProjectView : Form
    {
        private Project project = new Project();

        public string ProjectFile { get; private set; }
        public NewProjectView()
        {
            InitializeComponent();
        }

        private void ProjectPathButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = true;
                if(fbd.ShowDialog() == DialogResult.OK)
                {
                    ProjectPathText.Text = fbd.SelectedPath;
                    CheckEnableOkButton();
                }
            }
        }

        private void MainSourceButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "ASM | *.s; *.asm; *.68k; *.i";

                if(fileDialog.ShowDialog() == DialogResult.OK)
                {
                    MainSourceText.Text = fileDialog.FileName;
                    CheckEnableOkButton();
                }
            }
        }

        private void ProjectNameText_TextChanged(object sender, EventArgs e)
        {            
            CheckEnableOkButton();
        }

        private void CheckEnableOkButton()
        {
            if (ProjectNameText.Text != String.Empty && ProjectPathText.Text != String.Empty)
            {
                OkButton.Enabled = true;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {

            ProjectFile = $"{ProjectPathText.Text}\\{ProjectNameText.Text}.mdproj";

            project.Name = ProjectNameText.Text;
            project.Assembler = (Assembler)AssemblerSelection.SelectedIndex;
            project.MainSourceFile = MainSourceText.Text;

            var additionalFiles = AdditionFilesListView.Items;
            List<string> projectFiles = new List<string>();

            foreach (ListViewItem file in additionalFiles)
            {
                var newFile = file.Text.Remove(0, $"{ProjectPathText.Text}\\".Length);
                projectFiles.Add(newFile);    
            }

            project.SourceFiles = projectFiles.ToArray();

            project.Write(ProjectPathText.Text);
            if (String.IsNullOrEmpty(MainSourceText.Text))
            {
                project.MainSourceFile = "main.asm";
                File.WriteAllText($"{ProjectPathText.Text}\\main.asm", "; main file\n; write code here");
            }
        }
    }
}
