using System.Threading;
using System.Windows.Input;
using Android.Views;
using Android.Views.InputMethods;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewOnFocusOutBinding : BaseBinding<InstantAutoCompleteTextView, ICommand>
    {
        private ICommand command;

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

            autoComplete.FocusChange += this.OnFocusChange;
        }

        private void OnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if(!e.HasFocus) this.command.Execute(null);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.Target.FocusChange -= this.OnFocusChange;
            }
            base.Dispose(isDisposing);
        }
    }
}