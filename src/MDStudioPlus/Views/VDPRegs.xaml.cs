using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace MDStudioPlus.Views
{
    /// <summary>
    /// Interaction logic for VDPRegs.xaml
    /// </summary>
    public partial class VDPRegs : Window
    {
        public class Register
        {
            public string Name;
            public uint Value;
        }

        public ObservableCollection<Register> Registers { get; set; }      

        public VDPRegs()
        {
            InitializeComponent();

            Registers = new ObservableCollection<Register>()
            {
                new Register(){ Name = "D0", Value = 0x00},
                new Register(){ Name = "D1", Value = 0x01},
                new Register(){ Name = "D2", Value = 0x02},
                new Register(){ Name = "D3", Value = 0x03},
                new Register(){ Name = "D4", Value = 0x04},
                new Register(){ Name = "D5", Value = 0x05},
                new Register(){ Name = "D6", Value = 0x06},
            };
        }
    }
}
