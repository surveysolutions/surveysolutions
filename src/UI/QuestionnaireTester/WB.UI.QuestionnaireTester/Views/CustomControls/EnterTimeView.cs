using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace WB.UI.QuestionnaireTester.Views.CustomControls
{
    public class DatePickerDialogFragment : DialogFragment
    {
        private readonly Context context;
        private DateTime date;
        private readonly EventHandler<DatePickerDialog.DateSetEventArgs> handler;

        public DatePickerDialogFragment(Context context, DateTime date, EventHandler<DatePickerDialog.DateSetEventArgs> handler)
        {
            this.context = context;
            this.date = date;
            this.handler = handler;
        }

        public override Dialog OnCreateDialog(Bundle savedState)
        {
            var dialog = new DatePickerDialog(context, handler, date.Year, date.Month - 1, date.Day);
            return dialog;
        }
    }

    //public class EnterTimeView : MvxFragment, DatePickerDialog.IOnDateSetListener
    //{
    //    private EditText datePickerText;

    //    public EnterTimeView()
    //    {
    //        this.RetainInstance = true;
    //    }

    //    public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
    //    {
    //        this.HasOptionsMenu = true;

    //        base.OnCreateView(inflater, container, savedInstanceState);

    //        var view = inflater.Inflate(Resource.Layout.EnterTimeView, container, false);

    //        datePickerText = view.FindViewById<EditText>(Resource.Id.DatePickerEditText);
    //        datePickerText.Focusable = false;
    //        datePickerText.Click += delegate
    //        {
    //            var dialog = new DatePickerDialogFragment(Activity, Convert.ToDateTime(datePickerText.Text), this);
    //            dialog.Show(FragmentManager, "date");
    //        };

    //        var set = this.CreateBindingSet<EnterTimeView, DateTimeQuestionViewModel>();
    //        set.Bind(datePickerText).To(vm => vm.Answer);
    //        set.Apply();

    //        return view;
    //    }

    //    public void OnDateSet(Android.Widget.DatePicker view, int year, int monthOfYear, int dayOfMonth)
    //    {
    //        datePickerText.Text = new DateTime(year, monthOfYear + 1, dayOfMonth).ToString();
    //    }


    //}
}