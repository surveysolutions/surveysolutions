using System;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace CAPI.Android.Bindings
{
    public class VisibilityBinding: MvxBaseAndroidTargetBinding
    {
        private readonly View _control;
        public VisibilityBinding(View control)
        {
            _control = control;
        }
        #region Overrides of MvxBaseTargetBinding

        public override void SetValue(object value)
        {
            var visibility = (bool) value;
            _control.Visibility = visibility ? ViewStates.Visible : ViewStates.Gone;
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