using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.Content;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

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

        public override void OnStart()
        {
            base.OnStart();
            var datePicker = (DatePickerDialog) Dialog;
            int color = ContextCompat.GetColor(Context, Resource.Color.colorAccent);
            var colorColor = new Color(color);
            datePicker.GetButton((int) DialogButtonType.Positive).SetTextColor(colorColor);
            datePicker.GetButton((int) DialogButtonType.Negative).SetTextColor(colorColor);
        }
    }
}
