using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Widget;
using MvvmCross.Binding;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ToParentGroupButtonBinding : BaseBinding<Button, GroupStateViewModel>
    {
        public ToParentGroupButtonBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(Button control, GroupStateViewModel value)
        {
            SimpleGroupStatus status = value?.SimpleStatus ?? SimpleGroupStatus.Other;

            int groupBackgroundResourceId = GetGroupBackgroundResourceIdByStatus(status);

            control.SetBackgroundResource(groupBackgroundResourceId);

            int textColorId = GetTextColorId(status);
            var color = new Color(ContextCompat.GetColor(control.Context, textColorId));
            control.SetTextColor(color);

            var drawable = GetArrawDrawable(status);
            drawable.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
            control.SetCompoundDrawables(drawable, null, null, null);
        }

        private static int GetGroupBackgroundResourceIdByStatus(SimpleGroupStatus? status)
        {
            switch (status)
            {
                case SimpleGroupStatus.Completed:
                    return Resource.Drawable.to_parent_group_completed;

                case SimpleGroupStatus.Invalid:
                    return Resource.Drawable.to_parent_group_invalid;

                default:
                    return Resource.Drawable.to_parent_group_started;
            }
        }

        private static int GetTextColorId(SimpleGroupStatus? status)
        {
            switch (status)
            {
                case SimpleGroupStatus.Completed:
                    return Resource.Color.group_completed;

                case SimpleGroupStatus.Invalid:
                    return Resource.Color.group_with_invalid_answers;

                default:
                    return Resource.Color.group_started;
            }
        }

        private Drawable GetArrawDrawable(SimpleGroupStatus status)
        {
            Drawable drawable;

            switch (status)
            {
                case SimpleGroupStatus.Completed:
                    drawable = ContextCompat.GetDrawable(Target.Context, Resource.Drawable.back_to_parent_compeleted);
                    break;
                case SimpleGroupStatus.Invalid:
                    drawable = ContextCompat.GetDrawable(Target.Context, Resource.Drawable.back_to_parent_invalid);
                    break;
                default:
                    drawable = ContextCompat.GetDrawable(Target.Context, Resource.Drawable.back_to_parent);
                    break;
            }

            return ScaleImage(drawable);
        }

        private Drawable ScaleImage(Drawable image)
        {
            if (!(image is BitmapDrawable))
            {
                return image;
            }

            Bitmap b = ((BitmapDrawable)image).Bitmap;

            var desiredHeight = 35;
            var aspectRatio = (double)b.Width/b.Height;
            int sizeX = dpToPx((int)(desiredHeight * aspectRatio));
            int sizeY = dpToPx(desiredHeight);

            Bitmap bitmapResized = Bitmap.CreateScaledBitmap(b, sizeX, sizeY, false);

            image = new BitmapDrawable(Target.Resources, bitmapResized);
            return image;
        }
        public static int dpToPx(int dp)
        {
            return (int)(dp * Resources.System.DisplayMetrics.Density);
        }
    }
}
