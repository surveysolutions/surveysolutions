using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Target;

using Cirrious.MvvmCross.Binding.Interfaces;

namespace AndroidApp.Bindings
{
    public class ValidationMessageBinding : MvxBaseAndroidTargetBinding
    {
        private readonly TextView _control;
        public ValidationMessageBinding(TextView control)
        {
            _control = control;
        }
        #region Overrides of MvxBaseTargetBinding

        public override void SetValue(object value)
        {
            var validationStatus = (QuestionStatus) value;


            if (validationStatus.HasFlag(QuestionStatus.Valid))
            {
                _control.Visibility = ViewStates.Gone;

            }
            else
            {
                _control.Visibility = ViewStates.Visible;

            }
        }

        public override Type TargetType
        {
            get { return typeof(QuestionStatus); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        #endregion

    }
}