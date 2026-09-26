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
        private IDisposable subscription;
        
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

            subscription = target.WeakSubscribe(
                nameof(target.Click),
                HandleClick);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                subscription?.Dispose();
                subscription = null;
            }
            base.Dispose(isDisposing);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            if (Mvx.IoCProvider == null)
                return;

            if (!Mvx.IoCProvider.TryResolve(out IMvxAndroidCurrentTopActivity topActivity))
                return;

            var activity = topActivity.Activity;
            if (activity == null)
                return;

            var view = sender as View;
            if (view == null)
                return;

            activity.RemoveFocusFromEditText();
            activity.HideKeyboard(view.WindowToken);
        }
    }
}
