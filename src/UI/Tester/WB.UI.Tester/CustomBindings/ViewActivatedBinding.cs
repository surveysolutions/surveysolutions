using Android.Views;

namespace WB.UI.Tester.CustomBindings
{
    public class ViewActivatedBinding : BaseBinding<View, bool>
    {
        public ViewActivatedBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, bool value)
        {
            control.Activated = value;
        }
    }
}