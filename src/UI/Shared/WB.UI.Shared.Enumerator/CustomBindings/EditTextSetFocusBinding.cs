using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextSetFocusBinding : BaseBinding<EditText, bool>
    {
        public EditTextSetFocusBinding(EditText editText)
            : base(editText)
        {
            editText.ShowSoftInputOnFocus = true;
        }

        private EditText EditText
        {
            get { return this.Target; }
        }

        protected override void SetValueToView(EditText control, bool value)
        {
            var editText = control;
            if (editText == null)
                return;

            if (value)
            {
                editText.RequestFocus();
                InputMethodManager imm = (InputMethodManager)editText.Context.GetSystemService(Context.InputMethodService);
                imm.ShowSoftInput(editText, ShowFlags.Implicit);
            }
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if (this.EditText == null)
                return;

            this.EditText.FocusChange += this.HandleFocusChange;
            this.EditText.EditorAction += this.HandleEditorAction;
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;

            if (e.ActionId != ImeAction.Done) return;

            e.Handled = true;

            this.TriggerValueChangeToFalse();
        }

        private void HandleFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus) return;

            this.TriggerValueChangeToFalse();
        }

        private void TriggerValueChangeToFalse()
        {
            if (this.EditText == null)
                return;
            IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var currentTopActivity = topActivity.Activity;

            currentTopActivity.HideKeyboard(Target.WindowToken);
            this.FireValueChanged(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                if (this.EditText != null && this.EditText.Handle != IntPtr.Zero)
                {
                    this.EditText.FocusChange -= this.HandleFocusChange;
                    this.EditText.EditorAction -= this.HandleEditorAction;
                }
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;
    }
}
