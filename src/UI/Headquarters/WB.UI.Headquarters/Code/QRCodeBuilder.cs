using System;
using System.IO;
using ZXing.QrCode;

namespace WB.UI.Headquarters.Code
{
    public class QRCodeBuilder
    {
        public static string GetQRCodeAsBase64String(string content, int height = 250, int width = 250, int margin = 0)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            var qrWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions { Height = height, Width = width, Margin = margin}
            };

            var pixelData = qrWriter.Write(content);
            
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                       pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
