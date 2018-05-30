using System;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using MvvmCross.Platforms.Android;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

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
                BitmapFactory.Options boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true, InPurgeable = true };
                using (BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions)) // To determine actual image size
                {
                    int sampleSize = CalculateInSampleSize(boundsOptions, minSize, minSize);

                    var bitmapOptions = new BitmapFactory.Options {InSampleSize = sampleSize, InPurgeable = true};
                    using (var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, bitmapOptions))
                    {
                        if (bitmap != null)
                        {
                            SetupPaddingForImageView(control, displayMetrics, boundsOptions);
                            control.Drawable?.Dispose();
                            control.SetImageBitmap(bitmap);
                        }
                        else
                        {
                            this.SetDefaultImage(control);
                        }
                    }
                }
            }
            else
            {
                this.SetDefaultImage(control);
            }
        }

        protected virtual void SetDefaultImage(ImageView control)
        {
            control.SetImageResource(Resource.Drawable.img_placeholder);
        }

        private static void SetupPaddingForImageView(ImageView control, DisplayMetrics displayMetrics, BitmapFactory.Options boundsOptions)
        {
            int margin_left_dp = 0;
            int margin_right_dp = 0;
            int margin_top_dp = 0;
            int margin_bottom_dp = 0;
            var layout_width = LinearLayout.LayoutParams.WrapContent;
            var layout_height = LinearLayout.LayoutParams.WrapContent;

            var isNeedPadding = boundsOptions.OutWidth < displayMetrics.WidthPixels;
            if (isNeedPadding)
            {
                float element_margin_horizontal = control.Resources.GetDimension(Resource.Dimension.Interview_Entity_Element_margin_horizontal);
                float element_margin_left_dp = control.Resources.GetDimension(Resource.Dimension.Interview_Entity_margin_left);
                float element_margin_right_dp = control.Resources.GetDimension(Resource.Dimension.Interview_Entity_margin_right);
                margin_bottom_dp = (int)control.Resources.GetDimension(Resource.Dimension.Interview_Attachment_Small_margin_bottom);
                margin_left_dp = (int)(element_margin_left_dp + element_margin_horizontal);
                margin_right_dp = (int)(element_margin_right_dp + element_margin_horizontal);
            }
            else
            {
                margin_bottom_dp = (int)control.Resources.GetDimension(Resource.Dimension.Interview_Attachment_Large_margin_bottom);
                layout_width = LinearLayout.LayoutParams.MatchParent;
            }

            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(layout_width, layout_height);
            lp.SetMargins(margin_left_dp, margin_top_dp, margin_right_dp, margin_bottom_dp);
            control.LayoutParameters = lp;
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
