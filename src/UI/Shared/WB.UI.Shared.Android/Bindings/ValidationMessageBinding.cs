using System;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Shared.Android.Bindings
{
    public class ValidationMessageBinding : MvvmBindingWrapper<TextView>
    {
        public ValidationMessageBinding(TextView control):base(control)
        {
        }
        #region Overrides of MvxBaseTargetBinding


        protected override void SetValueToView(TextView view, object value)
        {
            var status = (QuestionStatus) value;

            var enabled = status.HasFlag(QuestionStatus.Enabled) && status.HasFlag(QuestionStatus.ParentEnabled);
            if (!status.HasFlag(QuestionStatus.Valid) && enabled)
            {
                view.Visibility = ViewStates.Visible;
            }
            else
            {
                view.Visibility = ViewStates.Gone;
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