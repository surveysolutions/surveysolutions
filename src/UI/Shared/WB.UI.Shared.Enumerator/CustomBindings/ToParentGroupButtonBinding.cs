using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using MvvmCross.Binding;
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
            control.SetTextColor(control.Resources.GetColor(textColorId));

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
                    return Resource.Drawable.group_with_invalid_answers;

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
            Drawable drawable = null; // control.Resources.GetDrawable(Resource.Drawable.back_to_parent);

            switch (status)
            {
                case SimpleGroupStatus.Completed:
                    drawable = Target.Resources.GetDrawable(Resource.Drawable.back_to_parent_completed);
                    break;
                case SimpleGroupStatus.Invalid:
                    break;
                default:
                    drawable = Target.Resources.GetDrawable(Resource.Drawable.back_to_parent);
                    break;
            }

            return ScaleImage(drawable, Target.Resources.DisplayMetrics.ScaledDensity/2);
        }

        private Drawable ScaleImage(Drawable image, double scaleFactor)
        {
            if (!(image is BitmapDrawable)) {
                return image;
            }

            Bitmap b = ((BitmapDrawable)image).Bitmap;

            int sizeX = (int) Math.Round(image.IntrinsicWidth * scaleFactor, 0);
            int sizeY = (int) Math.Round(image.IntrinsicHeight * scaleFactor, 0);

            Bitmap bitmapResized = Bitmap.CreateScaledBitmap(b, sizeX, sizeY, false);

            image = new BitmapDrawable(Target.Resources, bitmapResized);

            return image;
        }
    }
}