using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, GroupState>
    {
        private MvxBindingMode defaultMode;

        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        protected override void SetValueToView(Button control, GroupState value)
        {
            int groupBackgroundResourceId;

            switch (value.Status)
            {
                case GroupStatus.Completed:
                    groupBackgroundResourceId = Resource.Drawable.group_completed;
                    break;
                case GroupStatus.StartedInvalid:
                case GroupStatus.CompletedInvalid:
                    groupBackgroundResourceId = Resource.Drawable.group_with_invalid_answers;
                    break;
                default:
                    groupBackgroundResourceId = Resource.Drawable.group_started;
                    break;
            }

            control.SetBackgroundResource(groupBackgroundResourceId);
        }
    }
}