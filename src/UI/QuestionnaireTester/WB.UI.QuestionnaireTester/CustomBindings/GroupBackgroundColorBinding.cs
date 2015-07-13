using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Groups;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, GroupState>
    {
        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        } 

        protected override void SetValueToView(Button control, GroupState value)
        {
            GroupStatus? groupStatus = value != null ? value.Status : null as GroupStatus?;

            var groupBackgroundResourceId = GetGroupBackgroundResourceIdByState(groupStatus);

            control.SetBackgroundResource(groupBackgroundResourceId);
        }

        private static int GetGroupBackgroundResourceIdByState(GroupStatus? groupStatus)
        {
            switch (groupStatus)
            {
                case GroupStatus.Completed:
                    return Resource.Drawable.group_completed;

                case GroupStatus.StartedInvalid:
                case GroupStatus.CompletedInvalid:
                    return Resource.Drawable.group_with_invalid_answers;

                default:
                    return Resource.Drawable.group_started;
            }
        }
    }
}