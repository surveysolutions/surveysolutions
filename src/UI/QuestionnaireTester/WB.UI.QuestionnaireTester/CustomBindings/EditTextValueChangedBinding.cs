using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Text;
using Android.Util;
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

        private string oldEditTextValue;

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
            Target.FocusChange += FocusChange;
        }

        private void FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                oldEditTextValue = Target.Text;
            else
                TrySendAnswerTextQuestionCommand();
        }

        private void TrySendAnswerTextQuestionCommand()
        {
            var isTextChanged = oldEditTextValue != Target.Text;

            if (!isTextChanged)
                return;

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
                    Target.FocusChange -= FocusChange;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}