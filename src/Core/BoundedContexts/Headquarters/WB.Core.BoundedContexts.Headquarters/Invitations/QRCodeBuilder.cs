#nullable enable
using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.QrCode.Image;
using ZXing;
using ZXing.QrCode;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public static class QRCodeBuilder
    {
        public static string GetQRCodeAsBase64String(string content, int height = 250, int width = 250)
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

        public static MemoryStream RenderBarCodeImage(string text)
        {
            var width = 250;
            var height = 53;
            
            MultiFormatWriter writer = new MultiFormatWriter();
            var bm = writer.encode(text, BarcodeFormat.CODE_128, width, 1);
            int bmWidth = bm.Width;

            using SKBitmap bitmap = new SKBitmap(bmWidth, height);
            
            //using Bitmap imageBitmap = new Bitmap(bmWidth, height, PixelFormat.Format32bppRgb);

            for (int x = 0; x < bmWidth; x++) 
            {
                var color = bm[x, 0] ? SKColors.Black : SKColors.White;
                for (int y = 0; y < height; y++)
                    bitmap.SetPixel(x, y, color);
            }
            
            //imageBitmap.Save("c:\\Temp\\barcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
            
            data.SaveTo(ms);
            
            return ms;
        }

        public static MemoryStream RenderQrCodeImage(string text)
        {
            var width = 250;
            var height = 250;
            
            QRCodeWriter writer = new QRCodeWriter();
            var bm = writer.encode(text, BarcodeFormat.QR_CODE, width, height);
            int bmWidth = bm.Width;
            int bmHeight = bm.Height;

            using SKBitmap bitmap = new SKBitmap(bmWidth, height);
            for (int x = 0; x < bmWidth; x++) 
            {
                for (int y = 0; y < bmHeight; y++)
                {
                    var color = bm[x, y] ? SKColors.Black : SKColors.White;
                    bitmap.SetPixel(x, y, color);
                }
            }
            
            //imageBitmap.Save("c:\\Temp\\qrcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
            
            data.SaveTo(ms);
            
            return ms;
        }
    }
}
