using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android;
using MvvmCross.WeakSubscription;
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
        
        private IDisposable focusChangeSubscription;
        private IDisposable editorActionSubscription;

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

            focusChangeSubscription = this.EditText.WeakSubscribe<EditText, View.FocusChangeEventArgs>(
                nameof(this.EditText.FocusChange),
                this.HandleFocusChange);

            editorActionSubscription = this.EditText.WeakSubscribe<EditText, TextView.EditorActionEventArgs>(
                nameof(this.EditText.EditorAction),
                this.HandleEditorAction);
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
            IMvxAndroidCurrentTopActivity topActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var currentTopActivity = topActivity.Activity;

            currentTopActivity.HideKeyboard(Target.WindowToken);
            this.FireValueChanged(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.focusChangeSubscription?.Dispose();
                this.focusChangeSubscription = null;
                
                this.editorActionSubscription?.Dispose();
                this.editorActionSubscription = null;
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;
    }
}
