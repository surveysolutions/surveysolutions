using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.ViewModels;
using System;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextFocusValueChangedBinding : BaseBinding<EditText, IMvxCommand>
    {
        private IMvxCommand command;

        private string oldEditTextValue;

        public EditTextFocusValueChangedBinding(EditText target)
            : base(target)
        {
            target.ImeOptions = ImeAction.Done;
        }

        public override void SubscribeToEvents()
        {
            this.Target.FocusChange += FocusChange;
            this.Target.EditorAction += HandleEditorAction;
        }

        private void FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                oldEditTextValue = Target.Text;
            else
                TrySendAnswerTextQuestionCommand();
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;

            if (e.ActionId != ImeAction.Done) 
                return;

            e.Handled = true;

            TrySendAnswerTextQuestionCommand();

            HideKeyboard();
        }

        private void TrySendAnswerTextQuestionCommand()
        {
            var isTextChanged = oldEditTextValue != Target.Text;

            if (!isTextChanged)
                return;

            if (Target == null)
                return;

            if (Target.Visibility != ViewStates.Visible)
                return;

            if (this.command == null)
                return;

            if (!this.command.CanExecute())
                return;

            this.command.Execute(Target.Text);
            this.oldEditTextValue = Target.Text;
        }

        protected override void SetValueToView(EditText control, IMvxCommand value)
        {
            if (Target == null)
                return;

            this.command = value;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (Target != null)
                {
                    this.Target.FocusChange -= FocusChange;
                    this.Target.EditorAction -= HandleEditorAction;
                }
            }
            base.Dispose(isDisposing);
        }


        private void HideKeyboard()
        {
            var activity = (Activity)Target.Context;
            var windowToken = Target.WindowToken;

            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(windowToken, 0);
        }
    }
}