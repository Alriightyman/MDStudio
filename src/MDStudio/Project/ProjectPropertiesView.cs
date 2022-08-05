using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDStudio
{
    public partial class ProjectPropertiesView : Form
    {
        private Project project;
        private string projectPath = String.Empty;

        public ProjectPropertiesView(Project project,string projectPath)
        {
            InitializeComponent();
            this.projectPath = projectPath;
            this.project = project;
            PopulateProjectProperties();

            // on successful builds is default
            this.ShouldRun_Postbuild_Combo.SelectedIndex = 1;
        }

        private void PopulateProjectProperties()
        {
            Project_Name.Text = project.Name;
            this.Assembler_Choice.SelectedIndex = (int)project.Assembler;

            var options = project.AdditionalArguments;

            if(project.Assembler == Assembler.Asm68k)
            {
                this.asm68k_AssemblerOptions.Text = options;
                this.asm68k_AssemblerOptions.Enabled = true;
            }
            else
            {
                this.as_AssemblerOptions.Text = options;
                this.as_AssemblerOptions.Enabled = true; 
            }

            PreBuildCommands = project.PreBuildScript;
            PostBuildCommands = project.PostBuildScript;

            AuthorTextbox.Text = project.Author;

            if (project.FilesToExclude != null)
            {
                foreach (var item in project.FilesToExclude)
                {
                    ListViewItem lvItem = new ListViewItem();
                    lvItem.Text = item;

                    FilesToExcludeListView.Items.Add(lvItem);
                }
            }
        }

        public string PreBuildCommands { get { return this.Prebuild_Commands.Text; } set { this.Prebuild_Commands.Text = value; } }
        public string PostBuildCommands { get { return this.Postbuild_Commands.Text; } set { this.Postbuild_Commands.Text = value; } } 

        public bool PostBuildRunAlways
        {
            get
            {
                return this.ShouldRun_Postbuild_Combo.SelectedIndex == 0 ? true : false;
            }

            set
            {
                this.ShouldRun_Postbuild_Combo.SelectedIndex = value == true ? 0 : 1;
            }
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            project.Name = Project_Name.Text;
            project.PreBuildScript = PreBuildCommands;
            project.PostBuildScript = PostBuildCommands;

            List<string> files = new List<string>();
            int count = FilesToExcludeListView.Items.Count;
            for (int i = 0; i < count; i++)
            {
                files.Add(FilesToExcludeListView.Items[i].Text);
            }

            project.FilesToExclude = files.ToArray();
            
        }

        private void as_AssemblerOptions_TextChanged(object sender, EventArgs e)
        {
            project.AdditionalArguments = as_AssemblerOptions.Text;
        }

        private void asm68k_AssemblerOptions_TextChanged(object sender, EventArgs e)
        {
            project.AdditionalArguments = asm68k_AssemblerOptions.Text;
        }

        private void Postbuild_Commands_TextChanged(object sender, EventArgs e)
        {
            project.PostBuildScript = PostBuildCommands;
        }

        private void Prebuild_Commands_TextChanged(object sender, EventArgs e)
        {
            project.PreBuildScript = PreBuildCommands;
        }

        private void Assembler_Choice_SelectedIndexChanged(object sender, EventArgs e)
        {
            project.Assembler = (Assembler)Assembler_Choice.SelectedIndex;

            if (project.Assembler == Assembler.Asm68k)
            {
                asm68k_AssemblerOptions.Enabled = true;
                as_AssemblerOptions.Enabled = false;
            }
            else
            {
                asm68k_AssemblerOptions.Enabled = false;
                as_AssemblerOptions.Enabled = true;
            }
        }

        private void AuthorTextbox_TextChanged(object sender, EventArgs e)
        {
            project.Author = AuthorTextbox.Text;
        }

        private void AddFilesToExcludeButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Filter = "ASM|*.s;*.asm;*.68k;*.i";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                int length = $"{projectPath}\\".Length;
                string file = fileDialog.FileName.Remove(0, length);
                ListViewItem lvItem = new ListViewItem();
                lvItem.Text = file;
                FilesToExcludeListView.Items.Add(lvItem);                
            }
        }

        private void RemoveFilesToExcludeButton_Click(object sender, EventArgs e)
        {
            var selectedItems = FilesToExcludeListView.SelectedItems;

            foreach (ListViewItem item in selectedItems)
            {
                FilesToExcludeListView.Items.Remove(item);
            }
        }
    }
}
