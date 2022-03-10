using System;
using Android.App;
using Android.Widget;
using AndroidX.AppCompat.App;
using MvvmCross.Platforms.Android.Binding.Target;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.UI.Shared.Enumerator.CustomControls;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextDateBinding : MvxAndroidTargetBinding<EditText, DateTimeQuestionViewModel>
    {
        private DateTimeQuestionViewModel viewModel;

        public EditTextDateBinding(EditText androidControl) : base(androidControl)
        {
            this.Target.Click += this.InputClick;
        }

        private void InputClick(object sender, EventArgs args)
        {
            if (!DateTime.TryParse(this.Target.Text, out var parsedDate))
            {
                parsedDate = this.viewModel.DefaultDate ?? DateTime.Now;
            }

            var dialog = new DatePickerDialogFragment(parsedDate, this.OnDateSet);
            dialog.Show(((AppCompatActivity)Target.GetActivity()).SupportFragmentManager, "date");
        }

        private void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            if (this.Target != null)
            {
                this.viewModel.AnswerCommand?.Execute(e.Date);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.Target != null)
                {
                    this.Target.Click -= this.InputClick;
                }
            }
            
            base.Dispose(isDisposing);
        }

        protected override void SetValueImpl(EditText target, DateTimeQuestionViewModel value)
        {
            this.viewModel = value;
        }
    }
}
