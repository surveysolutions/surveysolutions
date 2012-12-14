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

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class DateQuestionView : AbstractQuestionView
    {
        public DateQuestionView(Context context, QuestionView model) : base(context, model)
        {
        }

        public DateQuestionView(Context context, IAttributeSet attrs, QuestionView model) : base(context, attrs, model)
        {
        }

        public DateQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionView model) : base(context, attrs, defStyle, model)
        {
        }

        public DateQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionView model) : base(javaReference, transfer, model)
        {
        }

        #region Overrides of AbstractQuestionView

        protected override void Initialize()
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.Context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.QuestionView_Date, this);

            // capture our View elements
            dateDisplay = FindViewById<TextView>(Resource.Id.dateDisplay);
            pickDate = FindViewById<Button>(Resource.Id.pickDate);
            tvTitle = this.FindViewById<TextView>(Resource.Id.tvTitle);
            dialog = new DatePickerDialog(this.Context, OnDateSet, date.Year, date.Month - 1, date.Day);
            tvTitle.Text = Model.Text;
            // add a click event handler to the button

            pickDate.Click += delegate
                {

                    dialog.Show();
                };
            if (!DateTime.TryParse(((ValueQuestionView) Model).Answer, out date))
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
        private TextView tvTitle;
        // const int DATE_DIALOG_ID = 0;
    }
}