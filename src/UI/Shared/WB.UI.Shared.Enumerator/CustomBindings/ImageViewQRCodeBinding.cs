using Android.Graphics;
using Android.Widget;
using ZXing;
using ZXing.Mobile;
using ZXing.QrCode;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ImageViewQRCodeBinding : BaseBinding<ImageView, string>
    {
        public ImageViewQRCodeBinding(ImageView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(ImageView control, string value)
        {
            if (value == null)
            {
                control.SetImageResource(Resource.Drawable.img_placeholder);
                return;
            }
            
            var writer = new QRCodeWriter();
            
            // 800px is just an good enough resolution for any QR Code to be readable and nice looking
            var qr = writer.encode(value, BarcodeFormat.QR_CODE, 800, 800);
            var bmr = new BitmapRenderer { Background = Color.Transparent };
            control.SetImageBitmap(bmr.Render(qr, BarcodeFormat.QR_CODE, value));
        }
    }
}
