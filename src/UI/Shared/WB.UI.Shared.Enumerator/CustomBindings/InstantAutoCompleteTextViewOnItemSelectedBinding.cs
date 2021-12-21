using System;
using System.Windows.Input;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewOnItemSelectedBinding : BaseBinding<InstantAutoCompleteTextView, ICommand>
    {
        private WeakReference<ICommand> command;
        private IDisposable subscription;

        public InstantAutoCompleteTextViewOnItemSelectedBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(InstantAutoCompleteTextView control, ICommand value)
        {
            if (this.Target == null)
                return;

            this.command = new WeakReference<ICommand>(value);
        }

        private void AutoCompleteOnSelectedObjectChanged(object sender, object selectedItem)
        {
            if (this.command != null && command.TryGetTarget(out var com))
                com.Execute(selectedItem);
        }

        public override void SubscribeToEvents()
        {
            var autoComplete = this.Target;

            if (autoComplete == null)
                return;

            this.subscription = autoComplete.WeakSubscribe<InstantAutoCompleteTextView, object>(
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
                this.subscription = null;
                command = null;
            }
            base.Dispose(isDisposing);
        }
    }
}
