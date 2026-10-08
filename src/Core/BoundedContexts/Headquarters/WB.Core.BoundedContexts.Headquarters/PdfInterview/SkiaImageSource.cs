using System;
using System.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SkiaSharp;
using WB.Core.Infrastructure.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview;

public class SkiaImageSource : ImageSource
{
    protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = 75)
        => Create(name, imageSource(), quality);

    protected override IImageSource FromFileImpl(string path, int? quality = 75)
        => Create(path, File.ReadAllBytes(path), quality);

    protected override IImageSource FromStreamImpl(string name, Func<Stream> imageStream, int? quality = 75)
    {
        using var stream = imageStream();
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return Create(name, buffer.ToArray(), quality);
    }

    private static IImageSource Create(string name, byte[] content, int? quality)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(content));
        if (codec == null)
            throw new InvalidImageException("Image format is not supported");

        // PDF bitmap importer expects non-premultiplied BGRA pixels
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        var bitmap = SKBitmap.Decode(codec, info);
        if (bitmap == null)
            throw new InvalidImageException("Image content is corrupted");

        return new SkiaImageSourceImpl(name, bitmap, quality ?? 75, codec.EncodedFormat == SKEncodedImageFormat.Png);
    }

    private class SkiaImageSourceImpl : IImageSource
    {
        private const int BmpHeaderSize = 54;

        private readonly SKBitmap bitmap;
        private readonly int quality;

        public SkiaImageSourceImpl(string name, SKBitmap bitmap, int quality, bool isTransparent)
        {
            this.Name = name;
            this.bitmap = bitmap;
            this.quality = quality;
            this.Transparent = isTransparent;
        }

        public int Width => bitmap.Width;

        public int Height => bitmap.Height;

        public string Name { get; }

        public bool Transparent { get; }

        public void SaveAsJpeg(MemoryStream ms)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            data.SaveTo(ms);
        }

        public void SaveAsPdfBitmap(MemoryStream ms)
        {
            var rowLength = bitmap.Width * 4;
            var pixelsLength = rowLength * bitmap.Height;

            using var writer = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true);
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write(BmpHeaderSize + pixelsLength);
            writer.Write(0);
            writer.Write(BmpHeaderSize);

            writer.Write(40); // BITMAPINFOHEADER
            writer.Write(bitmap.Width);
            writer.Write(bitmap.Height);
            writer.Write((short)1);
            writer.Write((short)32);
            writer.Write(0); // BI_RGB
            writer.Write(pixelsLength);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            // bitmap rows are stored bottom-up
            var pixels = bitmap.GetPixelSpan();
            for (var y = bitmap.Height - 1; y >= 0; y--)
                writer.Write(pixels.Slice(y * bitmap.RowBytes, rowLength));
        }

        public void Dispose() => bitmap.Dispose();
    }
}
