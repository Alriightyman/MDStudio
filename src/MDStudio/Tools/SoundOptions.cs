using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDStudio.Tools
{
    public partial class SoundOptions : Form
    {
        public int Volume
        {
            get
            {
                int volume = VolumeControl.Value;
                if (Mute_Volume.Checked)
                    volume = 0;
                return volume;
            }
        }
        public SoundOptions()
        {
            InitializeComponent();
        }
    }
}
