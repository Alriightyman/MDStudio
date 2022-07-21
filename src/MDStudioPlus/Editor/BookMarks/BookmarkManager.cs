using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Editor.BookMarks
{
    sealed class BookmarkManager //: IBookmarkManager
    {
        public static BookmarkManager Instance 
        { 
            get
            {
                if (instance == null)
                {
                    instance = new BookmarkManager();
                }

                return instance;
            }
            private set { instance = value; } 
        }

        private static BookmarkManager instance;

        public BookmarkManager()
        {
            Instance = this;
            //Project.ProjectService.SolutionClosed += delegate { Clear(); };
        }

        List<Bookmark> bookmarks = new List<Bookmark>();

        public IReadOnlyCollection<Bookmark> Bookmarks
        {
            get
            {				
                return bookmarks;
            }
        }

        public IEnumerable<Bookmark> GetBookmarks(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            return bookmarks.Where(b => b.FileName == fileName);
        }

        public void AddMark(Bookmark bookmark)
        {
            if (bookmark == null) return;
            if (bookmarks.Contains(bookmark)) return;
            if (bookmarks.Exists(b => IsEqualBookmark(b, bookmark))) return;
            bookmarks.Add(bookmark);
            OnAdded(new BookmarkEventArgs(bookmark));
        }

        public void AddMark(Bookmark bookmark, IDocument document, int line)
        {
            int lineStartOffset = document.GetLineByNumber(line).Offset;
            ISegment segment = TextUtilities.GetWhitespaceAfter(document, lineStartOffset);
            string textSegment = document.GetText(segment.Offset, segment.Length);
             
            int column = 1 + textSegment.Length;
            bookmark.Location = new TextLocation(line, column);
            bookmark.FileName = document.FileName;
            AddMark(bookmark);
        }

        static bool IsEqualBookmark(Bookmark a, Bookmark b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.GetType() != b.GetType())
                return false;
            if (a.FileName != b.FileName)
                return false;
            return a.LineNumber == b.LineNumber;
        }

        public void RemoveMark(Bookmark bookmark)
        {
            bookmarks.Remove(bookmark);
            OnRemoved(new BookmarkEventArgs(bookmark));
        }

        public void Clear()
        {
            while (bookmarks.Count > 0)
            {
                Bookmark b = bookmarks[bookmarks.Count - 1];
                bookmarks.RemoveAt(bookmarks.Count - 1);
                OnRemoved(new BookmarkEventArgs(b));
            }
        }

        void OnRemoved(BookmarkEventArgs e)
        {
            if (BookmarkRemoved != null)
            {
                BookmarkRemoved(null, e);
            }
        }

        void OnAdded(BookmarkEventArgs e)
        {
            if (BookmarkAdded != null)
            {
                BookmarkAdded(null, e);
            }
        }

        /*public IEnumerable<Bookmark> GetProjectBookmarks(ICSharpCode.SharpDevelop.Project.IProject project)
        {
            List<Bookmark> projectBookmarks = new List<Bookmark>();
            foreach (Bookmark mark in bookmarks)
            {
                // Only return those bookmarks which belong to the specified project.
                if (mark.IsSaved && mark.FileName != null && project.IsFileInProject(mark.FileName))
                {
                    projectBookmarks.Add(mark);
                }
            }
            return projectBookmarks;
        }*/

        public bool RemoveBookmarkAt(string fileName, int line, Predicate<Bookmark> predicate = null)
        {            
            foreach (Bookmark bookmark in GetBookmarks(fileName))
            {
                if (bookmark.CanToggle && bookmark.LineNumber == line)
                {
                    if (predicate == null || predicate(bookmark))
                    {
                        RemoveMark(bookmark);
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveAll(Predicate<Bookmark> match)
        {
            if (match == null)
                throw new ArgumentNullException("Predicate is null!");

            for (int index = bookmarks.Count - 1; index >= 0; --index)
            {
                Bookmark bookmark = bookmarks[index];
                if (match(bookmark))
                {
                    bookmarks.RemoveAt(index);
                    OnRemoved(new BookmarkEventArgs(bookmark));
                }
            }
        }

        public event EventHandler<BookmarkEventArgs> BookmarkRemoved;
        public event EventHandler<BookmarkEventArgs> BookmarkAdded;
    }
}
