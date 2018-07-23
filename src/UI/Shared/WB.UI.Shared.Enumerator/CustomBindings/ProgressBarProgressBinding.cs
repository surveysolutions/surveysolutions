using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ProgressBarProgressBinding : BaseBinding<ProgressBar, int>
    {
        public ProgressBarProgressBinding(ProgressBar target)
            : base(target)
        {
        }

        protected override void SetValueToView(ProgressBar view, int value) => this.Target.Progress = value;

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    }
}
