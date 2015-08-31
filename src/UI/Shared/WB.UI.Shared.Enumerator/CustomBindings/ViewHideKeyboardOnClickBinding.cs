using System;
using Android.App;
using Android.Views;
using Cirrious.MvvmCross.Binding;
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
            if (isDisposing && this.Target != null)
            {
                this.Target.Click -= this.HandleClick;
            }

            base.Dispose(isDisposing);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            var view = (View) sender;
            var activity = (Activity) view.Context;

            activity.RemoveFocusFromEditText();
            activity.HideKeyboard(view.WindowToken);
        }
    }
}