
namespace MDStudio
{
    partial class NewProjectView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewProjectView));
            this.label1 = new System.Windows.Forms.Label();
            this.ProjectNameText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.MainSourceButton = new System.Windows.Forms.Button();
            this.MainSourceText = new System.Windows.Forms.TextBox();
            this.ProjectPathText = new System.Windows.Forms.TextBox();
            this.AdditionFilesListView = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.AddFiles = new System.Windows.Forms.Button();
            this.RemoveFiles = new System.Windows.Forms.Button();
            this.ProjectPathButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.AssemblerSelection = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Name:";
            // 
            // ProjectNameText
            // 
            this.ProjectNameText.Location = new System.Drawing.Point(92, 34);
            this.ProjectNameText.Name = "ProjectNameText";
            this.ProjectNameText.Size = new System.Drawing.Size(264, 20);
            this.ProjectNameText.TabIndex = 1;
            this.ProjectNameText.TextChanged += new System.EventHandler(this.ProjectNameText_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(63, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Path:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(359, 49);
            this.label3.TabIndex = 3;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.MainSourceButton);
            this.groupBox1.Controls.Add(this.MainSourceText);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(35, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(371, 109);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Main Source File";
            // 
            // MainSourceButton
            // 
            this.MainSourceButton.Location = new System.Drawing.Point(336, 66);
            this.MainSourceButton.Name = "MainSourceButton";
            this.MainSourceButton.Size = new System.Drawing.Size(29, 23);
            this.MainSourceButton.TabIndex = 11;
            this.MainSourceButton.Text = "...";
            this.MainSourceButton.UseVisualStyleBackColor = true;
            this.MainSourceButton.Click += new System.EventHandler(this.MainSourceButton_Click);
            // 
            // MainSourceText
            // 
            this.MainSourceText.Location = new System.Drawing.Point(9, 68);
            this.MainSourceText.Name = "MainSourceText";
            this.MainSourceText.Size = new System.Drawing.Size(321, 20);
            this.MainSourceText.TabIndex = 4;
            // 
            // ProjectPathText
            // 
            this.ProjectPathText.Location = new System.Drawing.Point(101, 108);
            this.ProjectPathText.Name = "ProjectPathText";
            this.ProjectPathText.Size = new System.Drawing.Size(264, 20);
            this.ProjectPathText.TabIndex = 5;
            // 
            // AdditionFilesListView
            // 
            this.AdditionFilesListView.HideSelection = false;
            this.AdditionFilesListView.Location = new System.Drawing.Point(35, 290);
            this.AdditionFilesListView.Name = "AdditionFilesListView";
            this.AdditionFilesListView.Size = new System.Drawing.Size(371, 97);
            this.AdditionFilesListView.TabIndex = 6;
            this.AdditionFilesListView.UseCompatibleStateImageBehavior = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 271);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Add additional files";
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(35, 393);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(75, 23);
            this.AddFiles.TabIndex = 8;
            this.AddFiles.Text = "Add";
            this.AddFiles.UseVisualStyleBackColor = true;
            // 
            // RemoveFiles
            // 
            this.RemoveFiles.Location = new System.Drawing.Point(116, 393);
            this.RemoveFiles.Name = "RemoveFiles";
            this.RemoveFiles.Size = new System.Drawing.Size(75, 23);
            this.RemoveFiles.TabIndex = 9;
            this.RemoveFiles.Text = "Remove";
            this.RemoveFiles.UseVisualStyleBackColor = true;
            // 
            // ProjectPathButton
            // 
            this.ProjectPathButton.Location = new System.Drawing.Point(372, 106);
            this.ProjectPathButton.Name = "ProjectPathButton";
            this.ProjectPathButton.Size = new System.Drawing.Size(28, 23);
            this.ProjectPathButton.TabIndex = 10;
            this.ProjectPathButton.Text = "...";
            this.ProjectPathButton.UseVisualStyleBackColor = true;
            this.ProjectPathButton.Click += new System.EventHandler(this.ProjectPathButton_Click);
            // 
            // OkButton
            // 
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkButton.Enabled = false;
            this.OkButton.Location = new System.Drawing.Point(250, 442);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 11;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(331, 442);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 12;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Assembler:";
            // 
            // AssemblerSelection
            // 
            this.AssemblerSelection.FormattingEnabled = true;
            this.AssemblerSelection.Items.AddRange(new object[] {
            "Asm68k",
            "AS Macro"});
            this.AssemblerSelection.Location = new System.Drawing.Point(92, 63);
            this.AssemblerSelection.Name = "AssemblerSelection";
            this.AssemblerSelection.Size = new System.Drawing.Size(264, 21);
            this.AssemblerSelection.TabIndex = 14;
            // 
            // NewProjectView
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 508);
            this.Controls.Add(this.AssemblerSelection);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.ProjectPathButton);
            this.Controls.Add(this.RemoveFiles);
            this.Controls.Add(this.AddFiles);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.AdditionFilesListView);
            this.Controls.Add(this.ProjectPathText);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ProjectNameText);
            this.Controls.Add(this.label1);
            this.Name = "NewProjectView";
            this.Text = "New Project";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ProjectNameText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button MainSourceButton;
        private System.Windows.Forms.TextBox MainSourceText;
        private System.Windows.Forms.TextBox ProjectPathText;
        private System.Windows.Forms.ListView AdditionFilesListView;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button RemoveFiles;
        private System.Windows.Forms.Button ProjectPathButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox AssemblerSelection;
    }
}