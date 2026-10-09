using System;
using SkiaSharp;
using WB.Core.Infrastructure.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview;

public static class PdfImageSource
{
    private const int JpegQuality = 75;
    private const string Base64Prefix = "base64:";

    /// <summary>
    /// PDFsharp imports only a limited set of JPEG/PNG variants, so any image is decoded by SkiaSharp and
    /// re-encoded as baseline JPEG (PNG when transparency is possible) to be embedded to the document.
    /// </summary>
    /// <exception cref="InvalidImageException">Content is not a supported image</exception>
    public static string FromBinary(byte[] content)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(content));
        if (codec == null)
            throw new InvalidImageException("Image format is not supported");

        using var bitmap = SKBitmap.Decode(codec);
        if (bitmap == null)
            throw new InvalidImageException("Image content is corrupted");

        var keepTransparency = codec.EncodedFormat == SKEncodedImageFormat.Png;

        using var image = SKImage.FromBitmap(bitmap);
        using var data = keepTransparency
            ? image.Encode(SKEncodedImageFormat.Png, 100)
            : image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);

        if (data == null)
            throw new InvalidImageException("Image cannot be encoded");

        return Base64Prefix + Convert.ToBase64String(data.AsSpan());
    }
}
