using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.QrCode;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class BarCodeUtilities
    {
        public MemoryStream RenderBarCodeImage(string text)
        {
            var width = 250;
            var height = 53;
            
            MultiFormatWriter writer = new MultiFormatWriter();
            var bm = writer.encode(text, BarcodeFormat.CODE_128, width, 1);
            int bmWidth = bm.Width;

            using Bitmap imageBitmap = new Bitmap(bmWidth, height, PixelFormat.Format32bppRgb);

            for (int x = 0; x < bmWidth; x++) 
            {
                var color = bm[x, 0] ? Color.Black : Color.White;
                for (int y = 0; y < height; y++)
                    imageBitmap.SetPixel(x, y, color);
            }
            
            //imageBitmap.Save("c:\\Temp\\barcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            imageBitmap.Save(ms, ImageFormat.Jpeg);
            return ms;
        }

        public MemoryStream RenderQrCodeImage(string text)
        {
            var width = 250;
            var height = 250;
            
            QRCodeWriter writer = new QRCodeWriter();
            var bm = writer.encode(text, BarcodeFormat.QR_CODE, width, height);
            int bmWidth = bm.Width;
            int bmHeight = bm.Height;

            using Bitmap imageBitmap = new Bitmap(bmWidth, bmHeight, PixelFormat.Format32bppRgb);
            for (int x = 0; x < bmWidth; x++) 
            {
                for (int y = 0; y < bmHeight; y++)
                {
                    var color = bm[x, y] ? Color.Black : Color.White;
                    imageBitmap.SetPixel(x, y, color);
                }
            }
            
            //imageBitmap.Save("c:\\Temp\\qrcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            imageBitmap.Save(ms, ImageFormat.Jpeg);
            return ms;
        }
    }
}
