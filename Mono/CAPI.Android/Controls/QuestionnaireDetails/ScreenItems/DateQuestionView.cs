using System;
using Android.App;
using Android.Content;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class DateQuestionView : AbstractQuestionView
    {
     /*   public DateQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }
        */
        public DateQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            dateDisplay = new TextView(this.Context);
            llWrapper.AddView(dateDisplay);
            DateTime date;
            if (!DateTime.TryParse(Model.AnswerString, out date))
                date = DateTime.Now;
            dialog = new DatePickerDialog(this.Context, OnDateSet, date.Year, date.Month - 1, date.Day);
            llWrapper.Click += delegate
                {

                    dialog.Show();
                };

            this.PutAnswerStoredInModelToUI();
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            dateDisplay.Text = Model.AnswerString;
        }

        // the event received when the user "sets" the date in the dialog
        void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            string newValue = e.Date.ToString("d");
            if (newValue != this.Model.AnswerString)
            {
                dateDisplay.Text = newValue;

                this.SaveAnswer(
                    newValue,
                    new AnswerDateTimeQuestionCommand(this.QuestionnairePublicKey, CapiApplication.Membership.CurrentUser.Id,
                        Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow, e.Date));
            }
        }

        #endregion

        private TextView dateDisplay;
        private DatePickerDialog dialog;
    }
}