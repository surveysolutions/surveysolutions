using System;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class NumericValueBinding : BaseBinding<NumericEditText, decimal?>
    {
        private bool subscribed;

        public NumericValueBinding(NumericEditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            this.Target.NumericValueChanged += Target_NumericValueChanged;

            this.subscribed = true;
        }

        private void Target_NumericValueChanged(object sender, NumericValueChangedEventArgs e)
        {
            FireValueChanged(e.NewValue);
        }

        protected override void SetValueToView(NumericEditText view, decimal? value)
        {
            view.SetValue(value);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                var editText = this.Target;
                if (editText != null && this.subscribed && editText.Handle != IntPtr.Zero)
                {
                    editText.NumericValueChanged -= this.Target_NumericValueChanged;
                    this.subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
