using System;
using System.Windows.Input;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.WeakSubscription;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewOnFocusOutBinding : BaseBinding<InstantAutoCompleteTextView, ICommand>
    {
        private WeakReference<ICommand> command;
        private IDisposable subscribtion;

        public InstantAutoCompleteTextViewOnFocusOutBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(InstantAutoCompleteTextView control, ICommand value)
        {
            if (this.Target == null)
                return;

            this.command = new WeakReference<ICommand>(value);
        }

        public override void SubscribeToEvents()
        {
            var autoComplete = this.Target;
            if (autoComplete == null)
                return;

            subscribtion = autoComplete.WeakSubscribe<InstantAutoCompleteTextView, View.FocusChangeEventArgs>(nameof(autoComplete.FocusChange), this.OnFocusChange);
        }

        private void OnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus && this.command != null && this.command.TryGetTarget(out ICommand com))
            {
                com.Execute(null);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                this.subscribtion.Dispose();
                this.subscribtion = null;
                this.command = null;
            }
            base.Dispose(isDisposing);
        }
    }
}
