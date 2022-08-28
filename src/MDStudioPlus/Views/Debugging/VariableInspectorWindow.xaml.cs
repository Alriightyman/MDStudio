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

namespace MDStudioPlus.Views.Debugging
{
    /// <summary>
    /// Interaction logic for VariableInspectorWindow.xaml
    /// </summary>
    public partial class VariableInspectorWindow : Window
    {
        Window parentWindow;

        public VariableInspectorWindow(Window parent)
        {
            InitializeComponent();
            parentWindow = parent;
            this.Owner = parentWindow;

        }

        public void SetPosition(Point position)
        {
            this.Top = position.Y+10;
            this.Left = position.X+10;
        }
    }
}
