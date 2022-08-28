/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MDStudioPlus.Editor.BookMarks
{
    public sealed class BookmarkPad : BookmarkPadBase
    {
        public BookmarkPad()
        {
            *//*ToolBar toolbar = ToolBarService.CreateToolBar((UIElement)this.Control, this, "/SharpDevelop/Pads/BookmarkPad/Toolbar");
            this.control.Children.Add(toolbar);*//*
        }

        protected override bool ShowBookmarkInThisPad(Bookmark bookmark)
        {
            return bookmark.ShowInPad(this);
        }
    }

    public abstract class BookmarkPadBase //: AbstractPadContent
    {
        protected BookmarkPadContent control;

        public object Control
        {
            get { return this.control; }
        }

        public ListView ListView
        {
            get { return this.control.listView; }
        }

        public ItemCollection Items
        {
            get { return this.control.listView.Items; }
        }

        public Bookmark SelectedItem
        {
            get { return (Bookmark)this.control.listView.SelectedItem; }
        }

        public IEnumerable<Bookmark> SelectedItems
        {
            get { return this.control.listView.SelectedItems.OfType<Bookmark>(); }
        }

        protected BookmarkPadBase()
        {
            this.control = new BookmarkPadContent();
            this.control.InitializeComponent();

            SD.BookmarkManager.BookmarkAdded += BookmarkManagerAdded;
            SD.BookmarkManager.BookmarkRemoved += BookmarkManagerRemoved;

            foreach (Bookmark bookmark in SD.BookmarkManager.Bookmarks)
            {
                if (ShowBookmarkInThisPad(bookmark))
                {
                    this.Items.Add(bookmark);
                }
            }

            this.control.listView.MouseDoubleClick += delegate {
                SDBookmark bm = this.control.listView.SelectedItem as SDBookmark;
                if (bm != null)
                    OnItemActivated(bm);
            };

            this.control.listView.KeyDown += delegate (object sender, System.Windows.Input.KeyEventArgs e) {
                var selectedItems = this.SelectedItems.ToList();
                if (!selectedItems.Any())
                    return;
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Delete:
                        foreach (var selectedItem in selectedItems)
                        {
                            SD.BookmarkManager.RemoveMark(selectedItem);
                        }
                        break;
                }
            };
        }

        public void Dispose()
        {
            SD.BookmarkManager.BookmarkAdded -= BookmarkManagerAdded;
            SD.BookmarkManager.BookmarkRemoved -= BookmarkManagerRemoved;
        }

        protected abstract bool ShowBookmarkInThisPad(Bookmark mark);

        protected virtual void OnItemActivated(Bookmark bm)
        {
            FileService.JumpToFilePosition(bm.FileName, bm.LineNumber, 1);
        }

        void BookmarkManagerAdded(object sender, BookmarkEventArgs e)
        {
            if (ShowBookmarkInThisPad(e.Bookmark))
            {
                this.Items.Add(e.Bookmark);
            }
        }

        void BookmarkManagerRemoved(object sender, BookmarkEventArgs e)
        {
            this.Items.Remove(e.Bookmark);
        }

        public void BringToFront()
        {
            PadDescriptor d = this.PadDescriptor;
            if (d != null)
                d.BringPadToFront();
        }

        protected virtual PadDescriptor PadDescriptor
        {
            get
            {
                return SD.Workbench.GetPad(GetType());
            }
        }

        public virtual object InitiallyFocusedControl
        {
            get
            {
                return null;
            }
        }
    }
}
*/