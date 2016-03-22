using Android.Graphics;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Platform.Droid.Platform;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ImageViewBitmapWithFallbackBinding : BaseBinding<ImageView, byte[]>
    {
        public ImageViewBitmapWithFallbackBinding(ImageView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(ImageView control, byte[] value)
        {
            if (value != null)
            {
                var boundsOptions = new BitmapFactory.Options {InJustDecodeBounds = true};
                BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions);

                // Calculate inSampleSize
                int sampleSize = CalculateInSampleSize(boundsOptions, 500, 500);
                var bitmapOptions = new BitmapFactory.Options {InSampleSize = sampleSize};
                var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, bitmapOptions);

                if (bitmap != null)
                {
                    control.SetImageBitmap(bitmap);
                }
                else
                {
                    LoadDefaultImage(control);
                }
            }
            else
            {
                LoadDefaultImage(control);
            }
        }

        private static void LoadDefaultImage(ImageView control)
        {
            var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
            var resources = mvxAndroidCurrentTopActivity.Activity.Resources;
            var noImageOptions = new BitmapFactory.Options();
            var nullImageBitmap = BitmapFactory.DecodeResource(resources, Resource.Drawable.no_image_found, noImageOptions);

            control.SetImageBitmap(nullImageBitmap);
        }


        // http://stackoverflow.com/a/10127787/72174
        private static int CalculateInSampleSize(BitmapFactory.Options actualImageParams, int maxAllowedWidth, int maxAllowedHeight)
        {
            // Raw height and width of image
            int height = actualImageParams.OutHeight;
            int width = actualImageParams.OutWidth;
            int inSampleSize = 1;

            if (height > maxAllowedHeight || width > maxAllowedWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while (halfHeight / inSampleSize > maxAllowedHeight && halfWidth / inSampleSize > maxAllowedWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }
    }
}