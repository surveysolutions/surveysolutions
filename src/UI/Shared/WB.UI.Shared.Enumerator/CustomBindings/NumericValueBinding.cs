using System;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class NumericValueBinding : BaseBinding<NumericEditText, decimal?>
    {
        private IDisposable subscription;

        public NumericValueBinding(NumericEditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            var target = Target;
            if (target == null)
                return;
            
            subscription = target.WeakSubscribe<NumericEditText, NumericValueChangedEventArgs>(
                nameof(target.NumericValueChanged),
                Target_NumericValueChanged);
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
            if (isDisposing)
            {
                subscription?.Dispose();
                subscription = null;
            }
            base.Dispose(isDisposing);
        }
    }
}
