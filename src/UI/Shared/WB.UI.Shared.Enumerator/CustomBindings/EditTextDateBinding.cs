using System;
using Android.App;
using Android.Widget;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.Target;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextDateBinding : MvxAndroidTargetBinding
    {
        private IMvxAsyncCommand<DateTime> command;

        protected new EditText Target => (EditText)base.Target;

        public EditTextDateBinding(EditText androidControl) : base(androidControl)
        {
            this.Target.Click += this.InputClick;
        }

        public override Type TargetType => typeof(IMvxAsyncCommand<DateTime>);

        private void InputClick(object sender, EventArgs args)
        {
            DateTime parsedDate;
            if (!DateTime.TryParse(this.Target.Text, out parsedDate))
            {
                parsedDate = DateTime.Now;
            }

            var dialog = new DatePickerDialogFragment(parsedDate, this.OnDateSet);
            dialog.Show(Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity.FragmentManager, "date");
        }

        private void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            if (this.Target != null)
            {
                this.command?.Execute(e.Date);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                if (this.Target != null)
                {
                    this.Target.Click -= this.InputClick;
                }
            }
        }

        protected override void SetValueImpl(object target, object value)
        {
            this.command = (IMvxAsyncCommand<DateTime>)value;
        }
    }
}
