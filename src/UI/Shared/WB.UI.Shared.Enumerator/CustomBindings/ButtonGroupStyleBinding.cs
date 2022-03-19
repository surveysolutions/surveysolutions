using Android.Graphics;
using Android.Widget;
using AndroidX.Core.Content;
using MvvmCross.Binding;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, GroupStatus>
    {
        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(Button control, GroupStatus value)
        {
            var groupBackgroundResourceId = GetGroupBackgroundResourceIdByStatus(value);

            var color = ContextCompat.GetColor(control.Context, groupBackgroundResourceId);
            control.SetBackgroundColor(new Color(color));
        }

        private int GetGroupBackgroundResourceIdByStatus(GroupStatus status)
        {
            switch (status)
            {
                case GroupStatus.Completed:
                    return Resource.Color.group_completed;
                case GroupStatus.StartedInvalid:
                case GroupStatus.CompletedInvalid:
                    return Resource.Color.group_with_invalid_answers;
                default:
                    return Resource.Color.group_started;
            }
        }
        
    }
}
