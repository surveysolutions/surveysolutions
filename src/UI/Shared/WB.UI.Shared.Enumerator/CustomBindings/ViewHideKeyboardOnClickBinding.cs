using System;
using Android.Views;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewHideKeyboardOnClickBinding : BaseBinding<View, object>
    {
        public ViewHideKeyboardOnClickBinding(View view)
            : base(view) {}

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(View view, object value) { }

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.Click += this.HandleClick;
            }

            base.SubscribeToEvents();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing && this.Target != null && this.Target.Handle != IntPtr.Zero)
            {
                this.Target.Click -= this.HandleClick;
            }

            base.Dispose(isDisposing);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = topActivity.Activity;
            var view = (View) sender;

            activity.RemoveFocusFromEditText();
            activity.HideKeyboard(view.WindowToken);
        }
    }
}
