using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ButtonHasWarningOrSuccessBinding : BaseBinding<Button, bool>
    {
        public ButtonHasWarningOrSuccessBinding(Button androidControl) : base(androidControl) { }
        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
        protected override void SetValueToView(Button control, bool value)
        {
            var colorStateList = value 
                ? ContextCompat.GetColorStateList(control.Context, Resource.Color.button_warning_selector)
                : ContextCompat.GetColorStateList(control.Context, Resource.Color.button_success_selector);
            ViewCompat.SetBackgroundTintList(control, colorStateList);
        }
    }
}
