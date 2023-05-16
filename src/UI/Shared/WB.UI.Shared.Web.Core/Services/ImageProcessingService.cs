using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace WB.UI.Shared.Web.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        static ImageProcessingService()
        {
            SixLabors.ImageSharp.Configuration.Default.MemoryAllocator = MemoryAllocator.Default;
        }

        public void Validate(byte[] source)
        {
            using var _ = Image.Load(source);
        }

        public byte[] ResizeImage(byte[] source, int height, int width)
        {
            using var outputStream = new MemoryStream();
            using var image = Image.Load(source);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Position = AnchorPositionMode.Center,
                Mode = ResizeMode.Max,
                Size = new Size(width, height)
            }));

            image.SaveAsPng(outputStream);

            return outputStream.ToArray();
        }
    }
}
