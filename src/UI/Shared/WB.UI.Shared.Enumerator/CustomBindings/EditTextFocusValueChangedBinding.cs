﻿using System;
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
using WB.UI.Shared.Enumerator.Activities;
using Object = Java.Lang.Object;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextFocusValueChangedBinding : BaseBinding<EditText, ICommand>
    {
        private class InnerRunnable : Object, IRunnable
        {
            private readonly Action action;

            public InnerRunnable(Action action)
            {
                this.action = action;
            }

            public void Run()
            {
                this.action.Invoke();
            }
        }

        private ICommand command;

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
            if(IsDisposed)
                return;

            if (isDisposing)
            {
                if (this.Target != null && this.Target.Handle != IntPtr.Zero)
                {
                    this.Target.FocusChange -= this.FocusChange;
                    this.Target.EditorAction -= this.HandleEditorAction;
                }
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
