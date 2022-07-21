using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Editor.BookMarks
{
    /// <summary>
    /// Description of BookmarkEventHandler.
    /// </summary>
    public class BookmarkEventArgs : EventArgs
    {
        Bookmark bookmark;

        public Bookmark Bookmark
        {
            get
            {
                return bookmark;
            }
        }

        public BookmarkEventArgs(Bookmark bookmark)
        {
            this.bookmark = bookmark;
        }
    }
}
