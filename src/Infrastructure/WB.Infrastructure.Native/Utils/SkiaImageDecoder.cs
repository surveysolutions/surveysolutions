#nullable enable

using SkiaSharp;
using WB.Core.Infrastructure.Exceptions;

namespace WB.Infrastructure.Native.Utils;

public static class SkiaImageDecoder
{
    /// <summary>
    /// A decoded bitmap takes about 4 bytes per pixel, so the default budget limits memory of one decoded image to ~256 MB.
    /// Size of an image is declared in its header, a tiny highly compressed file can ask for gigabytes.
    /// </summary>
    public const long DefaultMaxPixels = 64_000_000;

    /// <summary>
    /// Reads format and size of the image from its header, pixels are not decoded and memory is not allocated.
    /// </summary>
    /// <exception cref="InvalidImageException">Content is not a supported image</exception>
    public static SKImageInfo ReadInfo(byte[] content, out SKEncodedImageFormat format)
    {
        using var codec = CreateCodec(content);
        format = codec.EncodedFormat;
        return codec.Info;
    }

    /// <summary>
    /// Decodes the whole image. Size from the header is checked before memory is allocated.
    /// </summary>
    /// <exception cref="InvalidImageException">
    /// Content is not a supported or corrupted image, or the image has more pixels than <paramref name="maxPixels"/>
    /// </exception>
    public static SKBitmap Decode(byte[] content, out SKEncodedImageFormat format, long maxPixels = DefaultMaxPixels)
    {
        using var codec = CreateCodec(content);
        format = codec.EncodedFormat;

        var info = codec.Info;
        if ((long)info.Width * info.Height > maxPixels)
            throw new InvalidImageException($"Image is too large: {info.Width}x{info.Height}");

        var bitmap = new SKBitmap();
        if (!bitmap.TryAllocPixels(info))
        {
            bitmap.Dispose();
            throw new InvalidImageException($"Image is too large: {info.Width}x{info.Height}");
        }

        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
        {
            bitmap.Dispose();
            throw new InvalidImageException("Image content is corrupted");
        }

        return bitmap;
    }

    private static SKCodec CreateCodec(byte[] content)
    {
        return SKCodec.Create(new SKMemoryStream(content))
               ?? throw new InvalidImageException("Image format is not supported");
    }
}
