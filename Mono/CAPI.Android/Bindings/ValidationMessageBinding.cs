using System;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace CAPI.Android.Bindings
{
    public class ValidationMessageBinding : MvxAndroidTargetBinding
    {
        private readonly TextView _control;
        public ValidationMessageBinding(TextView control):base(control)
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