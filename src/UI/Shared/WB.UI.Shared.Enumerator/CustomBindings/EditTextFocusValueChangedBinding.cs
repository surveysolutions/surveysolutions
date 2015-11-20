using Android.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomBindings
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
            this.Target.FocusChange += this.FocusChange;
            this.Target.EditorAction += this.HandleEditorAction;
        }

        private void FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                this.oldEditTextValue = this.Target.Text;
            }
            else
            {
                this.TrySendAnswerTextQuestionCommand();
                this.HideKeyboard(this.Target);
            }
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;

            if (e.ActionId != ImeAction.Done) 
                return;

            e.Handled = true;

            this.TrySendAnswerTextQuestionCommand();
            this.HideKeyboard(this.Target);
        }

        private void TrySendAnswerTextQuestionCommand()
        {
            var isTextChanged = this.oldEditTextValue != this.Target.Text;
            if (!isTextChanged)
                return;

            if (this.Target == null)
                return;

            if (this.Target.Visibility != ViewStates.Visible)
                return;

            if (this.command == null)
                return;

            if (!this.command.CanExecute())
                return;

            this.command.Execute(this.Target.Text);
            this.oldEditTextValue = this.Target.Text;
        }

        protected override void SetValueToView(EditText control, IMvxCommand value)
        {
            if (this.Target == null)
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
                if (this.Target != null)
                {
                    this.Target.FocusChange -= this.FocusChange;
                    this.Target.EditorAction -= this.HandleEditorAction;
                }
            }
            base.Dispose(isDisposing);
        }


        private void HideKeyboard(object sender)
        {
            IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = topActivity.Activity;
            activity.RemoveFocusFromEditText();

            var view = (View)sender;
            activity.HideKeyboard(view.WindowToken);
        }
    }
}