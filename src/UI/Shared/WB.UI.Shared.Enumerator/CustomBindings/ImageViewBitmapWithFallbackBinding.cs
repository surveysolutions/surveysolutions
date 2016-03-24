using System;
using Android.Graphics;
using Android.Util;
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
                var displayMetrics = GetDisplayMetrics();
                var minSize = Math.Min(displayMetrics.WidthPixels, displayMetrics.HeightPixels);

                // Calculate inSampleSize
                var boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions);
                int sampleSize = CalculateInSampleSize(boundsOptions, minSize, minSize);

                var bitmapOptions = new BitmapFactory.Options {InSampleSize = sampleSize};
                var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, bitmapOptions);

                if (bitmap != null)
                {
                    SetupPaddingForImageView(control, displayMetrics, boundsOptions);
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

        private static void SetupPaddingForImageView(ImageView control, DisplayMetrics displayMetrics, BitmapFactory.Options boundsOptions)
        {
            float margin_left_dp = 0;
            float margin_right_dp = 0;

            var isNeedPadding = boundsOptions.OutWidth < displayMetrics.WidthPixels;
            if (isNeedPadding)
            {
                var element_margin_horizontal = control.Resources.GetDimension(Resource.Dimension.Interview_Entity_Element_margin_horizontal);
                margin_left_dp = control.Resources.GetDimension(Resource.Dimension.Interview_Entity_margin_left);
                margin_right_dp = control.Resources.GetDimension(Resource.Dimension.Interview_Entity_margin_right);
                margin_left_dp += element_margin_horizontal;
                margin_right_dp += element_margin_horizontal;
            }

            control.SetPadding((int)margin_left_dp, 0, (int)margin_right_dp, 0);
        }

        private static DisplayMetrics GetDisplayMetrics()
        {
            var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
            var defaultDisplay = mvxAndroidCurrentTopActivity.Activity.WindowManager.DefaultDisplay;
            DisplayMetrics displayMetrics = new DisplayMetrics();
            defaultDisplay.GetMetrics(displayMetrics);
            return displayMetrics;
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
                while (halfHeight / inSampleSize > maxAllowedHeight || halfWidth / inSampleSize > maxAllowedWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }
    }
}