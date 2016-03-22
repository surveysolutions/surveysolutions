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
            }
            else
            {
                var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
                var resources = mvxAndroidCurrentTopActivity.Activity.Resources;
                var noImageOptions = new BitmapFactory.Options();
                var nullImageBitmap = BitmapFactory.DecodeResource(resources, Resource.Drawable.no_image_found, noImageOptions);

                control.SetImageBitmap(nullImageBitmap);
            }
        }

        private static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while (halfHeight / inSampleSize > reqHeight
                        && halfWidth / inSampleSize > reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }
    }
}