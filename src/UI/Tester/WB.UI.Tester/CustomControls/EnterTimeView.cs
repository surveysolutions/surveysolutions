using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace WB.UI.Tester.CustomControls
{
    public class DatePickerDialogFragment : DialogFragment
    {
        private readonly Context context;
        private DateTime date;
        private readonly EventHandler<DatePickerDialog.DateSetEventArgs> handler;

        public DatePickerDialogFragment()
        {
        }

        public DatePickerDialogFragment(Context context, DateTime date, EventHandler<DatePickerDialog.DateSetEventArgs> handler)
        {
            this.context = context;
            this.date = date;
            this.handler = handler;
        }

        public override Dialog OnCreateDialog(Bundle savedState)
        {
            var dialog = new DatePickerDialog(this.context, this.handler, this.date.Year, this.date.Month - 1, this.date.Day);
            return dialog;
        }
    }
}