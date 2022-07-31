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
            var UpdatedBookMarkArgs = new BookmarkEventArgs(bookmark);
            OnBeforeAdded(UpdatedBookMarkArgs);

            if(UpdatedBookMarkArgs.Bookmark.LineNumber < 0)
            {
                return;
            }

            if (bookmarks.Contains(UpdatedBookMarkArgs.Bookmark)) return;
            if (bookmarks.Exists(b => IsEqualBookmark(b, UpdatedBookMarkArgs.Bookmark))) return;
            bookmarks.Add(UpdatedBookMarkArgs.Bookmark);
            OnAdded(UpdatedBookMarkArgs);
            OnAddedAfter(UpdatedBookMarkArgs);
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
            OnBeforeRemoved(new BookmarkEventArgs(bookmark));
            bookmarks.Remove(bookmark);
            OnRemoved(new BookmarkEventArgs(bookmark));
        }

        public void Clear()
        {
            while (bookmarks.Count > 0)
            {
                Bookmark b = bookmarks[bookmarks.Count - 1];
                OnBeforeRemoved(new BookmarkEventArgs(b));
                bookmarks.RemoveAt(bookmarks.Count - 1);
                OnRemoved(new BookmarkEventArgs(b));
            }
        }

        void OnBeforeRemoved(BookmarkEventArgs e)
        {
            if (BookmarkBeforeRemoved != null)
            {
                BookmarkBeforeRemoved(null, e);
            }
        }

        void OnBeforeAdded(BookmarkEventArgs e)
        {
            if (BookmarkBeforeAdded != null)
            {
                BookmarkBeforeAdded(null, e);
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
        void OnAddedAfter(BookmarkEventArgs e)
        {
            BookmarkAfterAdded?.Invoke(null, e);
        }

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
                    OnBeforeRemoved(new BookmarkEventArgs(bookmark));
                    bookmarks.RemoveAt(index);
                    OnRemoved(new BookmarkEventArgs(bookmark));
                }
            }
        }

        public event EventHandler<BookmarkEventArgs> BookmarkRemoved;
        public event EventHandler<BookmarkEventArgs> BookmarkAdded;
        public event EventHandler<BookmarkEventArgs> BookmarkBeforeRemoved;
        public event EventHandler<BookmarkEventArgs> BookmarkBeforeAdded;
        public event EventHandler<BookmarkEventArgs> BookmarkAfterRemoved;
        public event EventHandler<BookmarkEventArgs> BookmarkAfterAdded;
    }
}
