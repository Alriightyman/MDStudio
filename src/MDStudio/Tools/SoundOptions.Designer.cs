
namespace MDStudio.Tools
{
    partial class SoundOptions
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
            this.label1 = new System.Windows.Forms.Label();
            this.VolumeControl = new System.Windows.Forms.TrackBar();
            this.Mute_Volume = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeControl)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Volume:";
            // 
            // VolumeControl
            // 
            this.VolumeControl.Location = new System.Drawing.Point(64, 28);
            this.VolumeControl.Maximum = 100;
            this.VolumeControl.Name = "VolumeControl";
            this.VolumeControl.Size = new System.Drawing.Size(104, 45);
            this.VolumeControl.TabIndex = 1;
            // 
            // Mute_Volume
            // 
            this.Mute_Volume.AutoSize = true;
            this.Mute_Volume.Location = new System.Drawing.Point(174, 33);
            this.Mute_Volume.Name = "Mute_Volume";
            this.Mute_Volume.Size = new System.Drawing.Size(50, 17);
            this.Mute_Volume.TabIndex = 2;
            this.Mute_Volume.Text = "Mute";
            this.Mute_Volume.UseVisualStyleBackColor = true;
            // 
            // SoundOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 450);
            this.Controls.Add(this.Mute_Volume);
            this.Controls.Add(this.VolumeControl);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SoundOptions";
            this.Text = "Sound Options";
            ((System.ComponentModel.ISupportInitialize)(this.VolumeControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar VolumeControl;
        private System.Windows.Forms.CheckBox Mute_Volume;
    }
}