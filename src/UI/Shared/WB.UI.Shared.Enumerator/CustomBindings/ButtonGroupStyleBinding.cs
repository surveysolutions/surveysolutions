using Android.Widget;
using MvvmCross.Binding;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, SimpleGroupStatus>
    {
        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        } 

        protected override void SetValueToView(Button control, SimpleGroupStatus value)
        {
            SimpleGroupStatus status = value;

            var groupBackgroundResourceId = GetGroupBackgroundResourceIdByStatus(status);

            control.SetBackgroundResource(groupBackgroundResourceId);
        }

        private static int GetGroupBackgroundResourceIdByStatus(SimpleGroupStatus? status)
        {
            switch (status)
            {
                case SimpleGroupStatus.Completed:
                    return Resource.Drawable.group_completed;

                case SimpleGroupStatus.Invalid:
                    return Resource.Drawable.group_with_invalid_answers;

                default:
                    return Resource.Drawable.group_started;
            }
        }
        
    }
}
