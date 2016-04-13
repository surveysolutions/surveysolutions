using System.Globalization;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class NumericValueBinding : BaseBinding<NumericEditText, double?>
    {
        private bool subscribed;

        public NumericValueBinding(NumericEditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            this.Target.NumericValueChanged += Target_NumericValueChanged;
            this.Target.NumericValueCleared += Target_NumericValueCleared;

            this.subscribed = true;
        }

        private void Target_NumericValueCleared(object sender, NumericValueClearedEventArgs e)
        {
            FireValueChanged(null);
        }

        private void Target_NumericValueChanged(object sender, NumericValueChangedEventArgs e)
        {
            FireValueChanged(e.NewValue);
        }

        protected override void SetValueToView(NumericEditText view, double? value)
        {
            view.Text = value?.ToString(CultureInfo.InvariantCulture);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = this.Target;
                if (editText != null && this.subscribed)
                {
                    editText.NumericValueChanged -= this.Target_NumericValueChanged;
                    editText.NumericValueCleared -= this.Target_NumericValueCleared;
                    this.subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
