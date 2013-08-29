using System;
using Android.App;
using Android.Content;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class DateQuestionView : AbstractQuestionView
    {
     /*   public DateQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }
        */
        public DateQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
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
           
            // add a click event handler to the button

            llWrapper.Click += delegate
                {

                    dialog.Show();
                };
          /*  if (!DateTime.TryParse(Model.AnswerString, out date))
                // get the current date
                date = DateTime.Today;*/

            // display the current date (this method is below)
            dateDisplay.Text = Model.AnswerString;
        }
        // the event received when the user "sets" the date in the dialog
        void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            string newValue = e.Date.ToString("d");
            if (newValue != this.Model.AnswerString)
            {
                CommandService.Execute(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                          null, newValue,
                                                          Model.PublicKey.PropagationKey));
                dateDisplay.Text = newValue;
            }
            
           // this.date = e.Date;
            SaveAnswer();
          //  UpdateDisplay();
        }
        #endregion

        private TextView dateDisplay;
       // private Button pickDate;
     //   private DateTime date=DateTime.Now;
        private DatePickerDialog dialog;
        // const int DATE_DIALOG_ID = 0;
    }
}