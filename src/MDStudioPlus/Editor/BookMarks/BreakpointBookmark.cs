using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MDStudioPlus.Editor.BookMarks
{
    public class BreakpointBookmark : MarkerBookmark, IHaveStateEnabled
    {
        bool isHealthy = true;
        bool isEnabled = true;
        string condition;

        public event EventHandler<EventArgs> ConditionChanged;

        public string Condition
        {
            get { return condition; }
            set
            {
                if (condition != value)
                {
                    condition = value;
                    if (ConditionChanged != null)
                        ConditionChanged(this, EventArgs.Empty);
                    Redraw();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object InternalBreakpointObject { get; set; }

        [DefaultValue(true)]
        public virtual bool IsHealthy
        {
            get
            {
                return isHealthy;
            }
            set
            {
                if (isHealthy != value)
                {
                    isHealthy = value;
                    Redraw();
                }
            }
        }

        [DefaultValue(true)]
        public virtual bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    if (IsEnabledChanged != null)
                        IsEnabledChanged(this, EventArgs.Empty);
                    Redraw();
                }
            }
        }

        public event EventHandler IsEnabledChanged;

        /// <summary>
        /// parameter-less ctor is necessary for deserialization
        /// </summary>
        public BreakpointBookmark()
        {
        }

        public BreakpointBookmark(string fileName, TextLocation location)
        {
            this.Location = location;
            this.FileName = fileName;
        }

        public static ImageSource BreakpointImage
        {
            get => new BitmapImage(new Uri("pack://application:,,,/Resources/Breakpoint.png"));            
        }
        public static ImageSource BreakpointConditionalImage
        {
            get => new BitmapImage(new Uri("pack://application:,,,/Resources/BreakpointConditional.png")); 
        }
        public static ImageSource DisabledBreakpointImage
        {
            get => new BitmapImage(new Uri("pack://application:,,,/Resources/DisabledBreakpoint.png"));
        }
        public static ImageSource UnhealthyBreakpointImage
        {
            get => new BitmapImage(new Uri("pack://application:,,,/Resources/UnhealthyBreakpoint.png")); 
        }
        public static ImageSource DisabledBreakpointConditionalImage
        {
            get => new BitmapImage(new Uri("pack://application:,,,/Resources/DisabledBreakpointConditional.png")); 
        }

        public override ImageSource Image
        {
            get
            {
                if (!this.IsEnabled)
                    return DisabledBreakpointImage;
                else if (this.IsHealthy)
                    return string.IsNullOrEmpty(this.Condition) ? BreakpointImage : BreakpointConditionalImage;
                else
                    return string.IsNullOrEmpty(this.Condition) ? UnhealthyBreakpointImage : DisabledBreakpointConditionalImage;
            }
        }

        protected override ITextMarker CreateMarker(ITextMarkerService markerService)
        {
            IDocumentLine line = this.Document.GetLineByNumber(this.LineNumber);
            ITextMarker marker = markerService.Create(line.Offset, line.Length);
            IHighlighter highlighter = this.Document.GetService(typeof(IHighlighter)) as IHighlighter;
            marker.BackgroundColor = BookmarkBase.BreakpointDefaultBackground;
            marker.ForegroundColor = BookmarkBase.BreakpointDefaultForeground;
            marker.MarkerColor = BookmarkBase.BreakpointDefaultBackground;
            marker.MarkerTypes = TextMarkerTypes.CircleInScrollBar;

            if (highlighter != null)
            {
                var color = highlighter.GetNamedColor(BookmarkBase.BreakpointMarkerName);
                if (color != null)
                {
                    marker.BackgroundColor = color.Background.GetColor(null);
                    marker.MarkerColor = color.Background.GetColor(null) ?? BookmarkBase.BreakpointDefaultForeground;
                    marker.ForegroundColor = color.Foreground.GetColor(null);
                }
            }
            return marker;
        }

        public override string ToString()
        {
            return string.Format("{0} @{1}", this.FileName, this.LineNumber);
        }

        public override bool DisplaysTooltip
        {
            get { return true; }
        }

        public override object CreateTooltipContent()
        {
            return null;
         /*       new BreakpointEditorPopup(this)
            {
                MinWidth = 300,
                StaysOpen = false
            };*/
        }
    }
}
