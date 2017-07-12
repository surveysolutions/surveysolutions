using System;
using System.Reflection;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using MvvmCross.Platform.Droid.WeakSubscription;
using MvvmCross.Platform.Platform;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewPartialTextTargetBinding : BaseBinding<InstantAutoCompleteTextView, string>
    {
        private IDisposable _subscription;

        public InstantAutoCompleteTextViewPartialTextTargetBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(InstantAutoCompleteTextView control, string value)
        {
            control.PartialText = value;
        }

        private void AutoCompleteOnPartialTextChanged(object sender, EventArgs eventArgs)
        {
            this.FireValueChanged(this.Target.PartialText);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public override void SubscribeToEvents()
        {
            var autoComplete = this.Target;
            if (autoComplete == null)
                return;

            this._subscription = autoComplete.WeakSubscribe(
                nameof(autoComplete.PartialTextChanged),
                AutoCompleteOnPartialTextChanged);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this._subscription?.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}