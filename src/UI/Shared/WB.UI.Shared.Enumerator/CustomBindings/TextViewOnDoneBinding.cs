using System;
using System.Windows.Input;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewOnDoneBinding : BaseBinding<TextView, ICommand>
    {
        private WeakReference<ICommand> command;

        public TextViewOnDoneBinding(TextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(TextView control, ICommand value)
        {
            if (this.Target == null)
                return;

            this.command = new WeakReference<ICommand>(value);
        }

        private void OnDone(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done) return;

            this.HideKeyboard();
            if (this.command != null && this.command.TryGetTarget(out var com))
                com.Execute(null);
        }

        private void HideKeyboard()
        {
            var androidContext = Target?.Context;
            if(androidContext == null) return;
            
            var ims = (InputMethodManager)androidContext.GetSystemService(Context.InputMethodService);
            ims.HideSoftInputFromWindow(this.Target.WindowToken, 0);
        }

        public override void SubscribeToEvents()
        {
            var textView = this.Target;

            if (textView == null)
                return;

            textView.EditorAction += this.OnDone;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                if (this.Target != null && this.Target.Handle != IntPtr.Zero)
                    this.Target.EditorAction -= this.OnDone;
                command = null;
            }
            base.Dispose(isDisposing);
        }
    }
}
