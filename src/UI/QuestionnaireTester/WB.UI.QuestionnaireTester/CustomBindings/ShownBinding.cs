using Android.Views;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ShownBinding : BaseBinding<View, bool>
    {
        public ShownBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, bool value)
        {
            control.Visibility = value ? ViewStates.Visible : ViewStates.Invisible;
        }
    }
}