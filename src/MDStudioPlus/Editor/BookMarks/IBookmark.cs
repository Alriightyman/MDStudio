using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MDStudioPlus.Editor.BookMarks
{
    /// <summary>
    /// Represents a bookmark in the bookmark margin.
    /// </summary>
    public interface IBookmark
    {
        /// <summary>
        /// Gets the line number of the bookmark.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the image.
        /// </summary>
        ImageSource Image { get; }

        /// <summary>
        /// Gets the Z-Order of the bookmark icon.
        /// </summary>
        int ZOrder { get; }

        /// <summary>
        /// Handles the mouse down event.
        /// </summary>
        void MouseDown(MouseButtonEventArgs e);

        /// <summary>
        /// Handles the mouse up event.
        /// </summary>
        void MouseUp(MouseButtonEventArgs e);

        /// <summary>
        /// Gets whether this bookmark can be dragged around.
        /// </summary>
        bool CanDragDrop { get; }

        /// <summary>
        /// Notifies the bookmark that it was dropped on the specified line.
        /// </summary>
        void Drop(int lineNumber);

        /// <summary>
        /// Gets whether this bookmark might want to display a tooltip.
        /// </summary>
        bool DisplaysTooltip { get; }

        /// <summary>
        /// Creates the tooltip content for the bookmark.
        /// </summary>
        object CreateTooltipContent();
    }
}
