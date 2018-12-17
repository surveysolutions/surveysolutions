using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ProgressBarIndeterminateModeBinding : BaseBinding<ProgressBar, bool>
    {
        public ProgressBarIndeterminateModeBinding(ProgressBar androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(ProgressBar control, bool value)
        {
            control.Indeterminate = value;
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    }
}
