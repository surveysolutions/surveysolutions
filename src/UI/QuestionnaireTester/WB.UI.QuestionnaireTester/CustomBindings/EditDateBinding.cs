using System;
using System.Globalization;
using System.Reflection;
using Android.App;
using Android.Views;
using Android.Widget;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.ViewModels;
using WB.UI.QuestionnaireTester.CustomControls;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditDateBinding : MvxAndroidTargetBinding
    {
        private IMvxCommand Command;

        protected new EditText Target
        {
            get { return (EditText)base.Target; }
        }

        public EditDateBinding(EditText androidControl) : base(androidControl)
        {
            Target.Click += InputClick;
        }

        public override Type TargetType
        {
            get { return typeof(IMvxCommand); }
        }

        private void InputClick(object sender, EventArgs args)
        {
            DateTime parsedDate;
            if (!DateTime.TryParse(Target.Text, out parsedDate))
            {
                parsedDate = DateTime.Now;
            };

            var dialog = new DatePickerDialogFragment(Target.Context, parsedDate, OnDateSet);
            Activity act = (Activity) Target.Context;

            dialog.Show(
                act.FragmentManager, 
                "date");
        }

        private void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            if (this.Target != null && this.Command != null)
            {
                this.Command.Execute(e.Date);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                if (Target != null)
                {
                    Target.Click -= InputClick;
                }
            }
        }

        protected override void SetValueImpl(object target, object value)
        {
            Command = (IMvxCommand)value;
        }
    }
}