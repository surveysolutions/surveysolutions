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
        private ICommand command;
        private IDisposable subscription;

        public InstantAutoCompleteTextViewOnFocusOutBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(InstantAutoCompleteTextView control, ICommand value)
        {
            if (this.Target == null)
                return;

            this.command = value;
        }

        public override void SubscribeToEvents()
        {
            var autoComplete = this.Target;
            if (autoComplete == null)
                return;

            subscription = autoComplete.WeakSubscribe<InstantAutoCompleteTextView, View.FocusChangeEventArgs>
                (nameof(autoComplete.FocusChange), this.OnFocusChange);
        }

        private void OnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                this.command?.Execute(null);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.subscription?.Dispose();
                this.subscription = null;
            }
            base.Dispose(isDisposing);
        }
    }
}
