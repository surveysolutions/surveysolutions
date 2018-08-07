using System;
using System.Windows.Input;
using Android.Views;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;
using MvvmCross.WeakSubscription;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ThrottledClickBinding : MvxAndroidTargetBinding
    {
       private ICommand command;
        private IDisposable clickSubscription;
        private IDisposable canExecuteSubscription;
        private readonly EventHandler<EventArgs> canExecuteEventHandler;

        private DateTime? lastClickUtc;

        protected View View => (View)Target;

        public ThrottledClickBinding(View view)
            : base(view)
        {
            canExecuteEventHandler = OnCanExecuteChanged;
            clickSubscription = view.WeakSubscribe(nameof(view.Click), ViewOnClick);
        }

        private void ViewOnClick(object sender, EventArgs args)
        {
            if (lastClickUtc.HasValue)
            {
                var timeSinceLastClickInMilliseconds = Math.Abs((lastClickUtc.Value - DateTime.UtcNow).TotalMilliseconds);

                if (timeSinceLastClickInMilliseconds < 500)
                {
                    return;
                }
            }

            if (command == null)
                return;

            if (!command.CanExecute(null))
                return;

            lastClickUtc = DateTime.UtcNow;
            command.Execute(null);
        }

        protected override void SetValueImpl(object target, object value)
        {
            canExecuteSubscription?.Dispose();
            canExecuteSubscription = null;

            command = value as ICommand;
            if (command != null)
            {
                canExecuteSubscription = command.WeakSubscribe(canExecuteEventHandler);
            }
            RefreshEnabledState();
        }

        private void RefreshEnabledState()
        {
            var view = View;
            if (view == null)
                return;

            var shouldBeEnabled = false;
            if (command != null)
            {
                shouldBeEnabled = command.CanExecute(null);
            }
            view.Enabled = shouldBeEnabled;
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            RefreshEnabledState();
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public override Type TargetType => typeof(ICommand);

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                clickSubscription?.Dispose();
                clickSubscription = null;

                canExecuteSubscription?.Dispose();
                canExecuteSubscription = null;
            }
            base.Dispose(isDisposing);
        }


    }
}
