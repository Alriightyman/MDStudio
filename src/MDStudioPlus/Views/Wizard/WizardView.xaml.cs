using MDStudioPlus.Models.Wizard;
using MDStudioPlus.Views.Wizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MDStudioPlus.Views
{
    /// <summary>
    /// Interaction logic for WizardView.xaml
    /// </summary>
    public partial class WizardView : NavigationWindow
    {
        public WizardView(WizardType pageType)
        {
            InitializeComponent();
            
            WizardLauncher wizardLauncher = new WizardLauncher(pageType);
            wizardLauncher.WizardReturn += wizardLauncher_WizardReturn;
            Navigate(wizardLauncher);
            
        }
        public WizardData WizardData { get; private set; }
        private void wizardLauncher_WizardReturn(object sender, WizardReturnEventArgs e)
        {
            // Handle wizard return
            WizardData = e.Data as WizardData;
            if (DialogResult == null)
            {
                // we want canceled to return false
                DialogResult = !(e.Result == WizardResult.Canceled);
                WizardData.Result = e.Result;
            }
        }
    }
}
