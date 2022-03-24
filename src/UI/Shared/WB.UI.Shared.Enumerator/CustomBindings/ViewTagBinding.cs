using Android.Views;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewTagBinding : BaseBinding<View, string>
    {
        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;

        public ViewTagBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, string value)
        {
            if (value == null || control.Tag != null || string.IsNullOrEmpty(value)) return;
            
            control.Tag = value;
            control.ContentDescription = value;
        }
    }
}
