using Android.Graphics;
using ZXing;
using ZXing.QrCode;

namespace WB.UI.Shared.Enumerator.Services
{
    public static class QRCodeRenderer 
    {
        public static Bitmap RenderToBitmap(string value, int width = 800)
        {
            var writer = new QRCodeWriter();
            var bitMatrix = writer.encode(value, BarcodeFormat.QR_CODE, width, width);
            
            // Create a bitmap from the bit matrix
            var bitmap = Bitmap.CreateBitmap(width, width, Bitmap.Config.Argb8888);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    bitmap.SetPixel(x, y, bitMatrix[x, y] ? Color.Black : Color.Transparent);
                }
            }
            
            return bitmap;
        }
    }
}
