using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class DateQuestionView : AbstractQuestionView
    {
     /*   public DateQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }
        */
        public DateQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source)
            : base(context, bindingActivity, source)
        {
        }

       /* public DateQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public DateQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            base.Initialize();
            // capture our View elements
            dateDisplay = new TextView(this.Context);
            llWrapper.AddView(dateDisplay);
            pickDate =new Button(this.Context);
            pickDate.Text = "Change the date";
            llWrapper.AddView(pickDate);
            dialog = new DatePickerDialog(this.Context, OnDateSet, date.Year, date.Month - 1, date.Day);
           
            // add a click event handler to the button

            pickDate.Click += delegate
                {

                    dialog.Show();
                };
            if (!DateTime.TryParse(Model.AnswerString, out date))
                // get the current date
                date = DateTime.Today;

            // display the current date (this method is below)
            UpdateDisplay();
        }

        // updates the date in the TextView
        private void UpdateDisplay()
        {
            dateDisplay.Text = date.ToString("d");
        }
        // the event received when the user "sets" the date in the dialog
        void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            this.date = e.Date;
            UpdateDisplay();
        }
 /*       protected override Dialog OnCreateDialog(int id)
        {
            switch (id)
            {
                case DATE_DIALOG_ID:
                    return new DatePickerDialog(this, OnDateSet, date.Year, date.Month - 1, date.Day);
            }
            return null;
        }*/
        #endregion

        private TextView dateDisplay;
        private Button pickDate;
        private DateTime date;
        private DatePickerDialog dialog;
        // const int DATE_DIALOG_ID = 0;
    }
}