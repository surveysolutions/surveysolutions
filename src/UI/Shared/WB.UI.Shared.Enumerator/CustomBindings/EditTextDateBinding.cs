using System;
using Android.App;
using Android.Widget;
using MvvmCross.Binding.Droid.Target;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextDateBinding : MvxAndroidTargetBinding
    {
        private IMvxCommand Command;

        protected new EditText Target => (EditText)base.Target;

        public EditTextDateBinding(EditText androidControl) : base(androidControl)
        {
            this.Target.Click += this.InputClick;
        }

        public override Type TargetType => typeof(IMvxCommand);

        private void InputClick(object sender, EventArgs args)
        {
            DateTime parsedDate;
            if (!DateTime.TryParse(this.Target.Text, out parsedDate))
            {
                parsedDate = DateTime.Now;
            }

            IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = topActivity.Activity;

            var dialog = new DatePickerDialogFragment(parsedDate, this.OnDateSet);
            dialog.Show(activity.FragmentManager, "date");
        }

        private void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            if (this.Target != null)
            {
                this.Command?.Execute(e.Date);
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
            this.Command = (IMvxCommand)value;
        }
    }
}