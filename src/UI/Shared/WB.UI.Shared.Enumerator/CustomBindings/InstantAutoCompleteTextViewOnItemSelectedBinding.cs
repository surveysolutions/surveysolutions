using System;
using System.Windows.Input;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewOnItemSelectedBinding : BaseBinding<InstantAutoCompleteTextView, ICommand>
    {
        private ICommand command;
        private IDisposable subscription;

        public InstantAutoCompleteTextViewOnItemSelectedBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(InstantAutoCompleteTextView control, ICommand value)
        {
            if (this.Target == null)
                return;

            this.command = value;
        }

        private void AutoCompleteOnSelectedObjectChanged(object sender, EventArgs eventArgs) 
            => command?.Execute(this.Target.SelectedObject);

        public override void SubscribeToEvents()
        {
            var autoComplete = this.Target;

            if (autoComplete == null)
                return;

            this.subscription = autoComplete.WeakSubscribe(
                nameof(autoComplete.SelectedObjectChanged),
                AutoCompleteOnSelectedObjectChanged);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                this.subscription?.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}
