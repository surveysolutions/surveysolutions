using System;
using SkiaSharp;
using WB.Core.Infrastructure.Exceptions;

namespace WB.UI.Shared.Web.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        public void Validate(byte[] source)
        {
            using var bitmap = Decode(source, out _);
        }

        public string GetImageMimeType(byte[] source)
        {
            using var bitmap = Decode(source, out var format);

            return format switch
            {
                SKEncodedImageFormat.Png => "image/png",
                SKEncodedImageFormat.Jpeg => "image/jpeg",
                SKEncodedImageFormat.Gif => "image/gif",
                SKEncodedImageFormat.Bmp => "image/bmp",
                SKEncodedImageFormat.Webp => "image/webp",
                SKEncodedImageFormat.Ico => "image/x-icon",
                SKEncodedImageFormat.Heif => "image/heif",
                SKEncodedImageFormat.Avif => "image/avif",
                _ => throw new InvalidImageException("Image format is not supported")
            };
        }

        public byte[] ResizeImage(byte[] source, int height, int width)
        {
            using var bitmap = Decode(source, out _);
            return Encode(FitInto(bitmap, width, height), SKEncodedImageFormat.Png);
        }

        public byte[] ResizeImageKeepingFormat(byte[] source, int size)
        {
            using var bitmap = Decode(source, out var format);

            // Skia cannot encode some decodable formats (e.g. GIF, BMP), PNG is used for them
            var targetFormat = format is SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp
                ? format
                : SKEncodedImageFormat.Png;

            return Encode(FitInto(bitmap, size, size), targetFormat);
        }

        private static SKBitmap Decode(byte[] source, out SKEncodedImageFormat format)
        {
            using var codec = SKCodec.Create(new SKMemoryStream(source));
            if (codec == null)
                throw new InvalidImageException("Image format is not supported");

            format = codec.EncodedFormat;

            var bitmap = SKBitmap.Decode(codec);
            if (bitmap == null)
                throw new InvalidImageException("Image content is corrupted");

            return bitmap;
        }

        private static SKBitmap FitInto(SKBitmap bitmap, int maxWidth, int maxHeight)
        {
            var ratio = Math.Min(maxWidth / (float)bitmap.Width, maxHeight / (float)bitmap.Height);
            var targetWidth = Math.Max(1, (int)Math.Round(bitmap.Width * ratio));
            var targetHeight = Math.Max(1, (int)Math.Round(bitmap.Height * ratio));

            var resized = bitmap.Resize(bitmap.Info.WithSize(targetWidth, targetHeight),
                new SKSamplingOptions(SKCubicResampler.Mitchell));

            return resized ?? throw new InvalidImageException("Image cannot be resized");
        }

        private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
        {
            using (bitmap)
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(format, 100))
            {
                if (data == null)
                    throw new InvalidImageException("Image cannot be encoded");

                return data.ToArray();
            }
        }
    }
}
