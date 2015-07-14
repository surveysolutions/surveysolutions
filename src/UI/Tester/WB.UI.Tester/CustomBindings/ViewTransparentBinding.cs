using Android.Views;

namespace WB.UI.Tester.CustomBindings
{
    public class ViewTransparentBinding : BaseBinding<View, bool>
    {
        public ViewTransparentBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, bool value)
        {
            control.Visibility = value ? ViewStates.Invisible : ViewStates.Visible;
        }
    }
}