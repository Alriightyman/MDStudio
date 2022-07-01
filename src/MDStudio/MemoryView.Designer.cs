namespace MDStudio
{
    partial class MemoryView
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
            this.m_ByteViewer = new System.ComponentModel.Design.ByteViewer();
            this.SuspendLayout();
            // 
            // m_ByteViewer
            // 
            this.m_ByteViewer.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.m_ByteViewer.ColumnCount = 1;
            this.m_ByteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ByteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ByteViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ByteViewer.Location = new System.Drawing.Point(0, 0);
            this.m_ByteViewer.Name = "m_ByteViewer";
            this.m_ByteViewer.RowCount = 1;
            this.m_ByteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ByteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ByteViewer.Size = new System.Drawing.Size(591, 419);
            this.m_ByteViewer.TabIndex = 1;
            this.m_ByteViewer.MouseHover += new System.EventHandler(this.m_ByteViewer_MouseHover);
            // 
            // MemoryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 419);
            this.Controls.Add(this.m_ByteViewer);
            this.Name = "MemoryView";
            this.Text = "MemoryView";
            this.ResumeLayout(false);

        }
        
        #endregion
        private System.ComponentModel.Design.ByteViewer m_ByteViewer;
    }
}