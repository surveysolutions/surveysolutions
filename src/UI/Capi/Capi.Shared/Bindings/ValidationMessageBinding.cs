using System;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Capi.Shared.Bindings
{
    public class ValidationMessageBinding : MvvmBindingWrapper<TextView>
    {
        public ValidationMessageBinding(TextView control):base(control)
        {
        }
        #region Overrides of MvxBaseTargetBinding


        protected override void SetValueToView(TextView view, object value)
        {
            var validationStatus = (QuestionStatus) value;


            if (validationStatus.HasFlag(QuestionStatus.Valid))
            {
                view.Visibility = ViewStates.Gone;
            }
            else
            {
                view.Visibility = ViewStates.Visible;
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