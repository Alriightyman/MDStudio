/*using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MDStudioPlus.Editor
{
    /// <summary>
    /// Represents an image.
    /// </summary>
    public interface IImage
    {
        /// <summary>
        /// Gets the image as WPF ImageSource.
        /// </summary>
        ImageSource ImageSource { get; }

        /// <summary>
        /// Gets the image as System.Drawing.Bitmap.
        /// </summary>
        Bitmap Bitmap { get; }

        /// <summary>
        /// Gets the image as System.Drawing.Icon.
        /// </summary>
        Icon Icon { get; }
    }

    /// <summary>
    /// Represents an image that gets loaded from a ResourceService.
    /// </summary>
    public class ResourceImage : IImage
    {
        readonly string resourceName;

        /// <summary>
        /// Creates a new ResourceServiceImage.
        /// </summary>
        /// <param name="resourceName">The name of the image resource.</param>       
        public ResourceServiceImage(string resourceName)
        {
            if (resourceName == null)
                throw new ArgumentNullException("resourceName");
            this.resourceName = resourceName;
        }

        internal ResourceServiceImage(IResourceService resourceService, string resourceName)
        {
            this.resourceName = resourceName;
        }

        /// <inheritdoc/>
        public ImageSource ImageSource
        {
            get
            {
                return PresentationResourceService.GetBitmapSource(resourceName);
            }
        }

        /// <inheritdoc/>
        public Bitmap Bitmap
        {
            get
            {                   
                return SD.ResourceService.GetBitmap(resourceName);
            }
        }

        /// <inheritdoc/>
        public Icon Icon
        {
            get
            {
                return SD.ResourceService.GetIcon(resourceName);
            }
        }

        public override bool Equals(object obj)
        {
            ResourceServiceImage other = obj as ResourceServiceImage;
            if (other == null)
                return false;
            return this.resourceName == other.resourceName;
        }

        public override int GetHashCode()
        {
            return resourceName.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[ResourceServiceImage {0}]", resourceName);
        }
    }
}
*/