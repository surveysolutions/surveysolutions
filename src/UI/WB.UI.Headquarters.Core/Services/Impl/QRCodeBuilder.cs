using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.QrCode.Image;

namespace WB.UI.Headquarters.Services.Impl
{
    public class QRCodeBuilder
    {
        public static string GetQRCodeAsBase64String(string content, int height = 250, int width = 250, int margin = 0)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            // generate QRCode
            var qrCode = new QrCode(content, new Vector2Slim(height, width), SKEncodedImageFormat.Png);

            // output to file
            using var output = new MemoryStream();
            qrCode.GenerateImage(output);
            return Convert.ToBase64String(output.ToArray());
        }
    }
}
