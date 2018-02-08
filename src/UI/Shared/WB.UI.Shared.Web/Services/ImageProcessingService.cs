using System.Drawing;
using System.IO;
using ImageResizer;

namespace WB.UI.Shared.Web.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        public void ValidateImage(byte[] source)
        {
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = 1,
                    MaxHeight = 1,
                    Format = "png"
                });

                outputStream.ToArray();
            }
        }

        public byte[] ResizeImage(byte[] source, int? height = null, int? width = null)
        {
            if (!height.HasValue || !width.HasValue) return source;

            //later should handle video and produce image preview 
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxHeight = height.Value,
                    MaxWidth = width.Value,
                    Format = "png",
                    Mode = FitMode.Max,
                    PaddingColor = Color.Transparent,
                    Anchor = ContentAlignment.MiddleCenter
                });

                return outputStream.ToArray();
            }
        }
    }
}