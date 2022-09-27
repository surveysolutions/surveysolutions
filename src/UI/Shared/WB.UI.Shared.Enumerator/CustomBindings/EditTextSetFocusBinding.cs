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
            var editText = EditText;
            if (editText == null)
                return;

            focusChangeSubscription = editText.WeakSubscribe<EditText, EditText.FocusChangeEventArgs>(
                nameof(editText.FocusChange),
                this.HandleFocusChange);

            editorActionSubscription = editText.WeakSubscribe<EditText, TextView.EditorActionEventArgs>(
                nameof(editText.EditorAction),
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
