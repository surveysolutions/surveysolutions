using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Platforms.Android;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ImageViewMarginsBitmapWithFallbackBinding : ImageViewBitmapWithFallbackBinding
    {
        public ImageViewMarginsBitmapWithFallbackBinding(ImageView androidControl) : base(androidControl)
        {
        }

        protected override void SetupImageView(ImageView control, DisplayMetrics displayMetrics, BitmapFactory.Options boundsOptions)
        {
            SetupMarginsForImageView(control, displayMetrics, boundsOptions);
        }

        private void SetupMarginsForImageView(ImageView control, DisplayMetrics displayMetrics, BitmapFactory.Options boundsOptions)
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
    }
}
