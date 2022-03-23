using System;
using System.Windows.Input;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Commands;
using MvvmCross.Platforms.Android;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.Activities;
using Object = Java.Lang.Object;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextFocusValueChangedBinding : BaseBinding<EditText, ICommand>
    {
        private class InnerRunnable : Object, IRunnable
        {
            private readonly WeakReference<Action> action;

            public InnerRunnable(Action action)
            {
                this.action = new WeakReference<Action>(action);
            }

            public void Run()
            {
                if (this.action.TryGetTarget(out var target))
                    target.Invoke();
            }
        }

        private ICommand command;
        private string oldEditTextValue;

        private IDisposable focusChangeSubscription;
        private IDisposable editorActionSubscription;

        public EditTextFocusValueChangedBinding(EditText target)
            : base(target)
        {
            target.ImeOptions = ImeAction.Done;
        }

        public override void SubscribeToEvents()
        {
            var target = Target;
            if (target == null)
                return;
            
            focusChangeSubscription = target.WeakSubscribe<EditText, View.FocusChangeEventArgs>(
                nameof(target.FocusChange),
                this.FocusChange);

            editorActionSubscription = target.WeakSubscribe<EditText, TextView.EditorActionEventArgs>(
                nameof(target.EditorAction),
                this.HandleEditorAction);
        }

        private void FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (this.Target != null)
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

            if (Target?.Visibility != ViewStates.Visible)
                return;

            if (this.command == null)
                return;

            if (this.command is IMvxCommand typedCommand && !typedCommand.CanExecute())
            {
                return;
            }
            if (this.command is IMvxCommand typedGenericCommand && !typedGenericCommand.CanExecute(this.Target.Text))
            {
                return;
            }

            var newValue = this.Target.Text;
            var fixCommand = this.command;

            this.Target.Post(new InnerRunnable( 
                () => {
                    fixCommand.Execute(newValue);
                    this.oldEditTextValue = newValue;
                }));
        }

        protected override void SetValueToView(EditText control, ICommand value)
        {
            if (this.Target == null)
                return;

            this.command = value;
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

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


        private void HideKeyboard(object sender)
        {
            IMvxAndroidCurrentTopActivity topActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = topActivity.Activity;
            activity.RemoveFocusFromEditText();

            var view = (View)sender;
            activity.HideKeyboard(view.WindowToken);
        }
    }
}
