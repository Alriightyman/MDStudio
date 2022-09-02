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
using MDStudioPlus.Editor.BookMarks;
using System.ComponentModel.Design;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace MDStudioPlus.Editor
{
    /// <summary>
    /// The text editor with margins for bookmarks, breakpoints as well as error markers
    /// </summary>
    public class CodeEditor : TextEditor
    {
        #region Static constructor
        // add the 2 versions of syntax highlighting (light/dark)
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
                        var highlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                                                                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting(name, new string[0], highlightingDefinition);
                    }
                }
            }
        }
        #endregion

        private SearchPanel searchPanel;
        private ITextMarkerService textMarkerService;
        private IconBarManager iconBarManager;
        private IconBarMargin iconBarMargin;
        public event BreakPointAddedEventHandler onBreakPointAdded;
        public event BreakPointRemovedEventHandler onBreakPointRemoved;
        public event BreakPointAddedEventHandler onBreakPointBeforeAdded;
        public event BreakPointRemovedEventHandler onBreakPointBeforeRemoved;
        public event BreakPointAddedEventHandler onBreakPointAfterAdded;

        /// <summary>
        /// Constructor
        /// <remarks>Adds a margin for breakpoints, search panel, and error markers</remarks>
        /// </summary>
        public CodeEditor() : base()
        {
            // add a new margin for breakpoints
            iconBarManager = new IconBarManager();
            iconBarMargin = new IconBarMargin(iconBarManager);
            iconBarMargin.onBreakPointRemoved -= Iconbar_onBreakPointRemoved;
            iconBarMargin.onBreakPointRemoved += Iconbar_onBreakPointRemoved;
            iconBarMargin.onBreakPointAdded -= Iconbar_onBreakPointAdded;
            iconBarMargin.onBreakPointAdded += Iconbar_onBreakPointAdded;

            iconBarMargin.onBreakPointBeforeRemoved -= Iconbar_onBreakPointBeforeRemoved;
            iconBarMargin.onBreakPointBeforeRemoved += Iconbar_onBreakPointBeforeRemoved;
            iconBarMargin.onBreakPointBeforeAdded -= Iconbar_onBreakPointBeforeAdded;
            iconBarMargin.onBreakPointBeforeAdded += Iconbar_onBreakPointBeforeAdded;
            iconBarMargin.onBreakPointAfterAdded -= Iconbar_onBreakPointAfterAdded;
            iconBarMargin.onBreakPointAfterAdded += Iconbar_onBreakPointAfterAdded;

            ShowLineNumbers = true;
            TextArea.LeftMargins.Insert(0, iconBarMargin);
            TextArea.LeftMargins.First().Visibility = Visibility.Visible;

            // add a search panel
            searchPanel = SearchPanel.Install(this.TextArea);
            //searchPanel.OverridesDefaultStyle = true;

            AddServices();
        }

        private void Iconbar_onBreakPointAfterAdded(object sender, BookmarkEventArgs e)
        {
            onBreakPointAfterAdded?.Invoke(this, e);
        }

        private void Iconbar_onBreakPointBeforeAdded(object sender, BookmarkEventArgs e)
        {
            onBreakPointBeforeAdded?.Invoke(this, e);
        }

        private void Iconbar_onBreakPointBeforeRemoved(object sender, BookmarkEventArgs e)
        {
            onBreakPointBeforeRemoved?.Invoke(this, e);
        }

        // this is a pass through to be able to wire up in an outside viewmodel
        private void Iconbar_onBreakPointAdded(object sender, BookmarkEventArgs e)
        {
            onBreakPointAdded?.Invoke(this, e);
        }

        // this is a pass through to be able to wire up in an outside viewmodel
        private void Iconbar_onBreakPointRemoved(object sender, BookmarkEventArgs e)
        {
            onBreakPointRemoved?.Invoke(sender, e);
        }

        // this seems to fire waaay to late
        protected override void OnDocumentChanged(EventArgs e)
        {
            base.OnDocumentChanged(e);
            AddTextMarkerService();
        }


        private void AddServices()
        {
            AddTextMarkerService();
            AddIconManager();
        }

        private void AddIconManager()
        {
            IServiceContainer services = (IServiceContainer)Document.ServiceProvider.GetService(typeof(IServiceContainer));
            services?.AddService(typeof(IBookmarkMargin), iconBarManager);
        }

        private void AddTextMarkerService()
        {
            // add the text marker service
            // NOTE: it must be done here since we don't have access to the
            // code editor in the FileViewModel
            if (Document != null)
            {
                var textMarkerService = new TextMarkerService(Document);
                TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
                TextArea.TextView.LineTransformers.Add(textMarkerService);
                IServiceContainer services = (IServiceContainer)Document.ServiceProvider.GetService(typeof(IServiceContainer));
                if (services != null && services.GetService(typeof(ITextMarkerService)) == null)
                    services.AddService(typeof(ITextMarkerService), textMarkerService);
                this.textMarkerService = textMarkerService;

                // add this editor as a "service" so we can access it elsewhere
                if (services != null && services.GetService(typeof(CodeEditor)) == null)
                    services.AddService(typeof(CodeEditor), this);
            }
        }

        #region Dependency Properties              

        #region IconBar Background
        /// <summary>
        /// Background property for the Iconbar margin
        /// </summary>
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
        #endregion

        #region IconBar Foreground
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

        #endregion

        #endregion

        #region Error Markers
        /// <summary>
        /// Removes all Error Markers from the document
        /// </summary>
        public void RemoveAllMarkers()
        {
            textMarkerService.RemoveAll(m => true);
        }

        /// <summary>
        /// Add Error marker at line number
        /// </summary>
        /// <param name="lineNumber">The line number where we want to add the marker to</param>
        public void AddMarker(int lineNumber, TextMarkerTypes type, Color markerColor)
        {
            var line = Document.GetLineByNumber(lineNumber);
            ITextMarker marker = textMarkerService.Create(line.Offset, line.Length);
            marker.MarkerTypes = type;
            marker.MarkerColor = markerColor;
        }

        #endregion

        #region Breakpoints
        /// <summary>
        /// Determines if a breakpoint is already set
        /// </summary>
        /// <param name="lineNumber">the line number the breakpoint is set at</param>
        /// <returns>true if one already exists, false if not</returns>
        public bool IsBreakpointSet(int lineNumber)
        {
            if (BookmarkManager.Instance.Bookmarks.Any(bookmark => bookmark.FileName == Document.FileName && bookmark.LineNumber == lineNumber))
            {
                // make sure the iconbar manager also has the bookmark set, otherwise it will not show up
                // in the text document UI
                var bm = BookmarkManager.Instance.Bookmarks.First(bookmark => bookmark.FileName == Document.FileName && bookmark.LineNumber == lineNumber);
                if (!this.iconBarManager.Bookmarks.Any(bookmark => bookmark.LineNumber == lineNumber))
                {
                    this.iconBarManager.Bookmarks.Add(bm);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Toggles break point on or off
        /// </summary>
        /// <param name="lineNumber">line number to set breakpoint at</param>
        public void ToggleBreakpoint(int lineNumber)
        {
            if (lineNumber > 0)
            {
                if (!BookmarkManager.Instance.RemoveBookmarkAt(Document.FileName, lineNumber, b => b is Bookmark))
                {
                    BookmarkManager.Instance.AddMark(new BreakpointBookmark(), Document, lineNumber);
                }
            }
        }

        public void ClearBreakpoints()
        {
            //this.iconBarManager.Bookmarks.Clear();
            BookmarkManager.Instance.RemoveAll(b => b is Bookmark);
        }

        #endregion

        public void Refresh()
        {
            InvalidateVisual();
            iconBarMargin.InvalidateVisual();
        }
    }
}
