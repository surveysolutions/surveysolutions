namespace WB.UI.Shared.Web.Services
{
    public interface IImageProcessingService
    {
        /// <exception cref="WB.Core.Infrastructure.Exceptions.InvalidImageException">Content is not a supported image</exception>
        void Validate(byte[] source);

        /// <summary>Validates the image and returns MIME type of its format</summary>
        /// <exception cref="WB.Core.Infrastructure.Exceptions.InvalidImageException">Content is not a supported image</exception>
        string GetImageMimeType(byte[] source);

        /// <summary>Fits image into width x height keeping aspect ratio and returns it as PNG</summary>
        /// <exception cref="WB.Core.Infrastructure.Exceptions.InvalidImageException">Content is not a supported image</exception>
        byte[] ResizeImage(byte[] source, int height, int width);

        /// <summary>Fits image into size x size keeping aspect ratio and original format (PNG when the format cannot be encoded)</summary>
        /// <exception cref="WB.Core.Infrastructure.Exceptions.InvalidImageException">Content is not a supported image</exception>
        byte[] ResizeImageKeepingFormat(byte[] source, int size);
    }
}
