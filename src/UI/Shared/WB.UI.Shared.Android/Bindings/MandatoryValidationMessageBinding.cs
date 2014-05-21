using System;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.Shared.Android.Bindings
{
    public class MandatoryValidationMessageBinding : MvvmBindingWrapper<TextView>
    {
        public MandatoryValidationMessageBinding(TextView control)
            : base(control)
        {
        }
        #region Overrides of MvxBaseTargetBinding


        protected override void SetValueToView(TextView view, object value)
        {
            var isMandatoryAndEmpty = (bool)value;

            view.Visibility = isMandatoryAndEmpty ? ViewStates.Visible : ViewStates.Gone;
        }

        public override Type TargetType
        {
            get { return typeof(bool); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        #endregion

    }
}