using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class DateQuestionView : AbstractQuestionView
    {
        public DateQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            this.dateDisplay = new TextView(this.Context);
            this.llWrapper.AddView(this.dateDisplay);
            DateTime date;
            if (!DateTime.TryParse(this.Model.AnswerString, out date))
                date = DateTime.Now;
            this.dialog = new DatePickerDialog(this.Context, this.OnDateSet, date.Year, date.Month - 1, date.Day);
            this.llWrapper.Click += delegate
                {

                    this.dialog.Show();
                };

            this.PutAnswerStoredInModelToUI();
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.dateDisplay.Text = this.GetAnswerStoredInModelAsString();
        }

        // the event received when the user "sets" the date in the dialog
        void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            string newValue = e.Date.ToString("d");
            if (newValue != this.Model.AnswerString)
            {
                this.dateDisplay.Text = newValue;

                this.SaveAnswer(
                    newValue,
                    new AnswerDateTimeQuestionCommand(this.QuestionnairePublicKey, this.Membership.CurrentUser.Id,
                        this.Model.PublicKey.Id, this.Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow, e.Date));
            }
        }

        #endregion

        private TextView dateDisplay;
        private DatePickerDialog dialog;
    }
}