using Android.Graphics;
using Android.Widget;
using ZXing;
using ZXing.Mobile;
using ZXing.QrCode;

namespace WB.UI.Shared.Enumerator.Services
{
    public static class QRCodeRenderer 
    {
        public static Bitmap RenderToBitmap(string value, int width = 800)
        {
            var writer = new QRCodeWriter();
            var qr = writer.encode(value, BarcodeFormat.QR_CODE, width, width);
            var bmr = new BitmapRenderer { Background = Color.Transparent };
            return bmr.Render(qr, BarcodeFormat.QR_CODE, value);
        }
    }
}
