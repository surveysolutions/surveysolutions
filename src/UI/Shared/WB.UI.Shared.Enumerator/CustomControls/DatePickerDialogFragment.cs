using System;
using Android.App;
using Android.OS;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class DatePickerDialogFragment : DialogFragment
    {
        private DateTime date;
        private readonly EventHandler<DatePickerDialog.DateSetEventArgs> handler;

        public DatePickerDialogFragment()
        {
        }

        public DatePickerDialogFragment(DateTime date, EventHandler<DatePickerDialog.DateSetEventArgs> handler)
        {
            this.date = date;
            this.handler = handler;
        }

        public override Dialog OnCreateDialog(Bundle savedState)
        {
            var dialog = new DatePickerDialog(this.Activity, this.handler, this.date.Year, this.date.Month - 1, this.date.Day);
            return dialog;
        }
    }
}