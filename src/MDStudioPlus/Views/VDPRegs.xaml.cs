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
using System.Windows.Shapes;

namespace MDStudioPlus.Views
{
    /// <summary>
    /// Interaction logic for VDPRegs.xaml
    /// </summary>
    public partial class VDPRegs : Window
    {
        private CheckBox[] checkBoxes;
        public VDPRegs()
        {
            InitializeComponent();

            checkBoxes = new CheckBox[]
            {
                checkBox7,checkBox6,checkBox5,checkBox4,checkBox3,checkBox2,checkBox1,checkBox0
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboReg.SelectedIndex = 0;
        }

        private void comboReg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var check in checkBoxes)
            {
                check.IsChecked = false;
                check.Content = "Unused";
            }

            switch (comboReg.SelectedIndex)
            {
                case 0:
                    checkBox4.Content = "IE1 (Horizontal interrupt enable)";
                    checkBox3.Content = "1 = invalid display setting";
                    checkBox2.Content = "Palette select";
                    checkBox1.Content = "M3(HV counter latch enable)";
                    checkBox0.Content = "Display disable";
                    break;

                case 1:
                    checkBox7.Content = "TMS9918 / Genesis display select";
                    checkBox6.Content = "DISP (Display Enable)";
                    checkBox5.Content = "IE0 (Vertical interrupt enable)";
                    checkBox4.Content = "M1 (DMA Enable)";
                    checkBox3.Content = "M2 (PAL/NTSC)";
                    checkBox2.Content = "SMS/Genesis display select";
                    checkBox0.Content = "?";
                    break;

                case 2:
                    checkBox5.Content = "Bit 15 name table address";
                    checkBox4.Content = "Bit 14 name table address";
                    checkBox3.Content = "Bit 13 name table address";

                    labelDesc.Text = "Pattern Name Tble Address for Plane A: $000";
                    break;

                case 3:
                    checkBox5.Content = "Bit 15 Pattern Name table address";
                    checkBox4.Content = "Bit 14 Pattern Name table address";
                    checkBox3.Content = "Bit 13 Pattern Name table address";
                    checkBox2.Content = "Bit 12 Pattern Name table address";
                    checkBox1.Content = "Bit 11 Pattern Name table address";

                    labelDesc.Text = "Pattern Name Table Address for Window: $000";
                    break;
            }

            int i = 7;
            foreach (CheckBox check in checkBoxes)
            {
                check.Content = "$" + i.ToString("X2") + ": " + check.Content;
                i--;
            }

            valueLabel.Text = "Value: $00";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
