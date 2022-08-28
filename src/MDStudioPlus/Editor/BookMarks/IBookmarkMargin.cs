using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Editor.BookMarks
{
    /// <summary>
    /// The bookmark margin.
    /// </summary>
    //[DocumentService]
    public interface IBookmarkMargin
    {
        /// <summary>
        /// Gets the list of bookmarks.
        /// </summary>
        IList<IBookmark> Bookmarks { get; }

        /// <summary>
        /// Redraws the bookmark margin. Bookmarks need to call this method when the Image changes.
        /// </summary>
        void Redraw();

        event EventHandler RedrawRequested;
    }
}
