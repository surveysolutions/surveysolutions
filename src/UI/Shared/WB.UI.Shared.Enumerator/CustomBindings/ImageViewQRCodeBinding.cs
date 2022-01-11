using Android.Widget;
using WB.UI.Shared.Enumerator.Services;

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

            // 800px is just an good enough resolution for any QR Code to be readable and nice looking
            var bitmap = QRCodeRenderer.RenderToBitmap(value, 800);
            control.SetImageBitmap(bitmap);
        }
    }
}
