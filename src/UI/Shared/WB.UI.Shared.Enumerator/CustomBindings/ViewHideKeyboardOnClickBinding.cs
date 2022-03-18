using System;
using Android.Views;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewHideKeyboardOnClickBinding : BaseBinding<View, object>
    {
        private IDisposable _subscription;
        
        public ViewHideKeyboardOnClickBinding(View view)
            : base(view) {}

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(View view, object value) { }

        public override void SubscribeToEvents()
        {
            var target = this.Target;
            if (target == null)
                return;

            _subscription = target.WeakSubscribe(
                nameof(target.Click),
                HandleClick);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _subscription?.Dispose();
                _subscription = null;
            }
            base.Dispose(isDisposing);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            IMvxAndroidCurrentTopActivity topActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = topActivity.Activity;
            var view = (View) sender;

            activity.RemoveFocusFromEditText();
            activity.HideKeyboard(view.WindowToken);
        }
    }
}
