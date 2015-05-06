using System;
using System.Windows.Input;
using Android.Text;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.ViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextValueChangedBinding : MvxAndroidTargetBinding
    {
        private IMvxCommand Command;

        private bool isFocused = false;
        private bool wasFocused = false;
        private bool isTextChanged = false;

        protected new EditText Target
        {
            get { return (EditText)base.Target; }
        }

        public EditTextValueChangedBinding(EditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Target.AfterTextChanged += AfterTextChanged;
            Target.FocusChange += FocusChange;
        }

        private void FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            this.wasFocused = isFocused && e.HasFocus == false;
            this.isFocused = e.HasFocus;

            TrySendAnswerTextQuestionCommand();
        }

        private void AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            this.isTextChanged = true;

            TrySendAnswerTextQuestionCommand();
        }

        private void TrySendAnswerTextQuestionCommand()
        {
            if (!this.wasFocused || !this.isTextChanged)
                return;

            this.isTextChanged = false;
            this.wasFocused = false;

            if (Target == null)
                return;

            if (Command == null)
                return;

            if (!Command.CanExecute())
                return;

            Command.Execute(Target.Text);
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (Target == null)
                return;

            Command = (IMvxCommand)value;
        }

        public override Type TargetType
        {
            get { return typeof(IMvxCommand); }
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
                    Target.AfterTextChanged -= AfterTextChanged;
                    Target.FocusChange -= FocusChange;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}