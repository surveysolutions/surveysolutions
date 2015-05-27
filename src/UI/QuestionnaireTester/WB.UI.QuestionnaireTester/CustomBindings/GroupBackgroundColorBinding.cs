using Android.Widget;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, GroupViewModel>
    {
        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(Button control, GroupViewModel value)
        {
            if (value == null) return;

            int groupBackgroundResourceId;

            if (!value.Enablement.Enabled) return;

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