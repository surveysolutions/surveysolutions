using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class DateQuestionView : AbstractQuestionView
    {
        private TextView dateDisplay;
        private DatePickerDialog dialog;
        private DateTime selectedDate;

        public DateQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            var hasAnswer = this.Model.AnswerObject is DateTime;

            selectedDate = hasAnswer ? (DateTime) this.Model.AnswerObject : DateTime.Now;

            var viewParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);
            viewParams.SetMargins(10, 20, 250, 0);

            dateDisplay = new TextView(this.CurrentContext) { LayoutParameters = viewParams };
            dateDisplay.SetTextSize(ComplexUnitType.Dip, 20);

            dialog = new DatePickerDialog(this.CurrentContext, this.OnDateSet, selectedDate.Year, selectedDate.Month - 1, selectedDate.Day);

            InitializeViewAndButtonView(this.dateDisplay, "Select date", this.ShowDateTimePicker);

            if (hasAnswer)
                PutAnswerStoredInModelToUI();
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.dateDisplay.Text = AnswerUtils.AnswerToString(selectedDate);
        }

        // the event received when the user "sets" the date in the dialog

        void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            if (e.Date != this.selectedDate)
            {
                selectedDate = e.Date;
                PutAnswerStoredInModelToUI();

                this.SaveAnswer(this.dateDisplay.Text,
                    new AnswerDateTimeQuestionCommand(this.QuestionnairePublicKey, 
                        this.Membership.CurrentUser.Id, 
                        this.Model.PublicKey.Id, 
                        this.Model.PublicKey.InterviewItemPropagationVector, 
                        DateTime.UtcNow, 
                        e.Date));
            }
        }

        private void ShowDateTimePicker(object sender, EventArgs e)
        {
            this.dialog.Show();
        }
    }
}