using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Editor.BookMarks
{
    /// <summary>
    /// A bookmark that is persistant across SharpDevelop sessions and has a text marker assigned to it.
    /// </summary>
    public abstract class MarkerBookmark : Bookmark
    {
        ITextMarker marker;

        protected abstract ITextMarker CreateMarker(ITextMarkerService markerService);

        public void SetMarker()
        {
            RemoveMarker();
            if (this.Document != null)
            {
                ITextMarkerService markerService = this.Document.GetService(typeof(ITextMarkerService)) as ITextMarkerService;
                if (markerService != null)
                {
                    marker = CreateMarker(markerService);
                }
            }
        }

        protected override void OnDocumentChanged(EventArgs e)
        {
            base.OnDocumentChanged(e);
            SetMarker();
        }

        public virtual void RemoveMarker()
        {
            if (marker != null)
            {
                marker.Delete();
                marker = null;
            }
        }
    }
}
