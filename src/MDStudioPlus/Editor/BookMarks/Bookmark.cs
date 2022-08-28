using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MDStudioPlus.Editor.BookMarks
{
    /// <summary>
    /// A bookmark that is persistant across SharpDevelop sessions.
    /// </summary>
    public abstract class Bookmark : BookmarkBase
    {
        string fileName;

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnFileNameChanged(EventArgs.Empty);
                }
            }
        }

        public string FileNameAndLineNumber
        {
            // TODO: kinda fixed?
            get { return string.Format("{0}:{1}", Path.GetFileName(this.FileName), this.LineNumber); }
        }

        public event EventHandler FileNameChanged;

        protected virtual void OnFileNameChanged(EventArgs e)
        {
            if (FileNameChanged != null)
            {
                FileNameChanged(this, e);
            }
        }

        public event EventHandler LineNumberChanged;

        internal void RaiseLineNumberChanged()
        {
            if (LineNumberChanged != null)
                LineNumberChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets/Sets if the bookmark should be saved to the project memento file.
        /// </summary>
        /// <remarks>
        /// Default is true; override this property if you are using the bookmark for
        /// something special like like "CurrentLineBookmark" in the debugger.
        /// </remarks>
        public virtual bool IsSaved
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether the bookmark should be displayed in the given pad.
        /// </summary>
/*        public virtual bool ShowInPad(BookmarkPadBase pad)
        {
            return true;
        }*/

        protected override void RemoveMark()
        {
            BookmarkManager.Instance.RemoveMark(this);
        }
    }
}
