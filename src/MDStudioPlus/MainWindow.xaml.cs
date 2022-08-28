using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using MDStudioPlus.Editor;
using MDStudioPlus.Editor.BookMarks;
using MDStudioPlus.Targets;
using MDStudioPlus.ViewModels;
using MDStudioPlus.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace MDStudioPlus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Workspace.Instance;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var cmdlineArgs = Environment.GetCommandLineArgs();
            Workspace.Instance.WindowLoaded(cmdlineArgs);
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void RestoreDownClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            SetCaptionHeight();
        }

        private void HeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetCaptionHeight();
        }

        private void SetCaptionHeight()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    chrome.CaptionHeight = header.ActualHeight + BorderThickness.Top - chrome.ResizeBorderThickness.Top;
                    break;
                case WindowState.Maximized:
                    chrome.CaptionHeight = header.ActualHeight - BorderThickness.Top;
                    break;
            }
        }

        // as much as I don't want to do this, I need to update the active document
        // so that the error markers show up on the document since the order of 
        // events isn't what I'd like it to be. 
        private void codeEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Workspace.Instance.UpdateDocumentErrors();
        }
    }
}
