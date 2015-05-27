using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
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
                this.Target.Click += HandleClick;
            }

            base.SubscribeToEvents();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && this.Target != null)
            {
                this.Target.Click -= HandleClick;
            }

            base.Dispose(isDisposing);
        }

        private static void HandleClick(object sender, EventArgs e)
        {
            var view = (View) sender;
            var activity = (Activity) view.Context;

            RemoveFocusFromEditText(activity);
            HideKeyboard(activity, view.WindowToken);
        }

        private static void RemoveFocusFromEditText(Activity activity)
        {
            View viewWithFocus = activity.CurrentFocus;

            if (viewWithFocus is EditText)
            {
                viewWithFocus.ClearFocus();
            }
        }

        private static void HideKeyboard(Activity activity, IBinder windowToken)
        {
            var inputMethodManager = (InputMethodManager) activity.GetSystemService(Context.InputMethodService);

            inputMethodManager.HideSoftInputFromWindow(windowToken, 0);
        }
    }
}