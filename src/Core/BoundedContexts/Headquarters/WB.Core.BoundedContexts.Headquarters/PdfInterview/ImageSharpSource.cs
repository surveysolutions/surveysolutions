using System;
using System.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview;

public class ImageSharpSource<TPixel> : ImageSource where TPixel : unmanaged, IPixel<TPixel>
{
    public static ImageSource.IImageSource FromImageSharpImage(
        Image<TPixel> image,
        IImageFormat imgFormat,
        int? quality = 75)
    {
        return (ImageSource.IImageSource)new ImageSharpSource<TPixel>.ImageSharpSourceImpl<TPixel>(
            "*" + Guid.NewGuid().ToString("B"), image, quality, imgFormat is PngFormat);
    }

    protected override ImageSource.IImageSource FromBinaryImpl(
        string name,
        Func<byte[]> imageSource,
        int? quality = 75)
    {
        var bytes = imageSource();
        IImageFormat format = Image.DetectFormat(bytes);
        Image<TPixel> image = Image.Load<TPixel>(bytes);
        return (ImageSource.IImageSource)new ImageSharpSource<TPixel>.ImageSharpSourceImpl<TPixel>(name, image,
            quality, format is PngFormat);
    }

    protected override ImageSource.IImageSource FromFileImpl(string path, int? quality = 75)
    {
        IImageFormat format = Image.DetectFormat(path);
        Image<TPixel> image = Image.Load<TPixel>(path);
        return (ImageSource.IImageSource)new ImageSharpSource<TPixel>.ImageSharpSourceImpl<TPixel>(path, image,
            quality, format is PngFormat);
    }

    protected override ImageSource.IImageSource FromStreamImpl(
        string name,
        Func<Stream> imageStream,
        int? quality = 75)
    {
        using (Stream stream = imageStream())
        {
            IImageFormat format = Image.DetectFormat(stream);
            Image<TPixel> image = Image.Load<TPixel>(stream);
            return (ImageSource.IImageSource)new ImageSharpSource<TPixel>.ImageSharpSourceImpl<TPixel>(name, image,
                quality, format is PngFormat);
        }
    }

    private class ImageSharpSourceImpl<TPixel2> : ImageSource.IImageSource where TPixel2 : unmanaged, IPixel<TPixel2>
    {
        private readonly int _quality;

        private Image<TPixel2> Image { get; }

        public int Width => this.Image.Width;

        public int Height => this.Image.Height;

        public string Name { get; }

        public bool Transparent { get; internal set; }

        public ImageSharpSourceImpl(
            string name,
            Image<TPixel2> image,
            int? quality,
            bool isTransparent)
        {
            this.Name = name;
            this.Image = image;
            this._quality = quality ?? 75;
            this.Transparent = isTransparent;
        }

        public void SaveAsJpeg(MemoryStream ms) => this.Image.SaveAsJpeg((Stream)ms, new JpegEncoder()
        {
            Quality = new int?(this._quality)
        });

        public void Dispose() => this.Image.Dispose();

        public void SaveAsPdfBitmap(MemoryStream ms)
        {
            BmpEncoder encoder = new BmpEncoder()
            {
                BitsPerPixel = new BmpBitsPerPixel?(BmpBitsPerPixel.Pixel32)
            };
            this.Image.Save((Stream)ms, (IImageEncoder)encoder);
        }
    }
}
