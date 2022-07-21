using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Resources;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Search;

namespace MDStudioPlus.Editor
{
    /// <summary>
    /// The text editor with margins for bookarks and breakpoints
    /// </summary>
    class CodeEditor : TextEditor
    {
        static CodeEditor()
        {
            AddHighlightFile(new Uri("pack://application:,,,/Resources/SyntaxHighlighting/Asm68k.xshd"), "ASM68K");
            AddHighlightFile(new Uri("pack://application:,,,/Resources/SyntaxHighlighting/Asm68kDark.xshd"), "ASM68KDark");
        }

        private static void AddHighlightFile(Uri uri, string name)
        {
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            if (sri != null)
            {
                using (Stream s = sri.Stream)
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        var hl = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                                      ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting(name, new string[0], hl);
                    }
                }
            }
        }
        private SearchPanel SearchPanel { get; set; }
        public CodeEditor() : base()
        {
            var iconbar = new IconBarMargin(new IconBarManager());
            ShowLineNumbers = true;
            TextArea.LeftMargins.Insert(0, iconbar);
            TextArea.LeftMargins.First().Visibility = Visibility.Visible;

            SearchPanel = SearchPanel.Install(this.TextArea);
            SearchPanel.OverridesDefaultStyle = true;
        }
        public static readonly DependencyProperty IconBarBackgroundProperty =
            DependencyProperty.Register("IconBarBackground", typeof(SolidColorBrush), typeof(CodeEditor), 
                                new FrameworkPropertyMetadata(Brushes.Red, OnIconBarMarginBackgroundChanged));

        public Brush IconBarBackground
        {
            get { return (Brush)GetValue(IconBarBackgroundProperty); }
            set { SetValue(IconBarBackgroundProperty, value); }
        }

        static void OnIconBarMarginBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CodeEditor editor = (CodeEditor)d;
            var iconBarMargin = editor.TextArea.LeftMargins.FirstOrDefault(margin => margin is IconBarMargin) as IconBarMargin; ;

            if (iconBarMargin != null)
            {
                iconBarMargin.SetValue(Control.BackgroundProperty, e.NewValue);
            }
        }

        public static readonly DependencyProperty IconBarForegroundProperty =
            DependencyProperty.Register("IconBarForeground", typeof(SolidColorBrush), typeof(CodeEditor),
                                new FrameworkPropertyMetadata(Brushes.Red, OnIconBarMarginForegroundChanged));

        public Brush IconBarForeground
        {
            get { return (Brush)GetValue(IconBarForegroundProperty); }
            set { SetValue(IconBarForegroundProperty, value); }
        }

        static void OnIconBarMarginForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CodeEditor editor = (CodeEditor)d;
            var iconBarMargin = editor.TextArea.LeftMargins.FirstOrDefault(margin => margin is IconBarMargin) as IconBarMargin; ;

            if (iconBarMargin != null)
            {
                iconBarMargin.SetValue(Control.ForegroundProperty, e.NewValue);
            }
        }
    }
}
