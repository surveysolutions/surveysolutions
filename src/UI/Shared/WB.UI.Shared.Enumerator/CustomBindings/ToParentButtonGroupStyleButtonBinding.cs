using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using AndroidX.Core.Content;
using Google.Android.Material.Button;
using MvvmCross.Binding;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ToParentButtonGroupStyleButtonBinding : BaseBinding<MaterialButton, GroupStateViewModel>
    {
        public ToParentButtonGroupStyleButtonBinding(MaterialButton androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(MaterialButton control, GroupStateViewModel value)
        {
            var resourceColorId = GetTextColorId(value?.SimpleStatus);
            var colorResourceId = ContextCompat.GetColor(control.Context, resourceColorId);
            var color = new Color(colorResourceId);
            control.SetTextColor(color);
            control.StrokeColor = ColorStateList.ValueOf(color);
            control.IconTint = ColorStateList.ValueOf(color);
            color.A = 85;
            control.RippleColor = ColorStateList.ValueOf(color);
        }

        private int GetTextColorId(SimpleGroupStatus? status)
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
    }
}
