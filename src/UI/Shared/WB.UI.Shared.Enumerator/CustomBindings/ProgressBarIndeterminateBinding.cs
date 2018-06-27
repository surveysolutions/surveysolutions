using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ProgressBarIndeterminateBinding : BaseBinding<ProgressBar, bool>
    {
        public ProgressBarIndeterminateBinding(ProgressBar target)
            : base(target)
        {
        }

        protected override void SetValueToView(ProgressBar view, bool value)
        {
            this.Target.Indeterminate = value;
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    }
}
