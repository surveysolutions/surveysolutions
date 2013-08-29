using System;
using Android.Views;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace CAPI.Android.Bindings
{
    public class VisibilityBinding : MvvmBindingWrapper<View>
    {
        public VisibilityBinding(View control):base(control)
        {
        }


        #region Overrides of MvxBaseTargetBinding

        protected override void SetValueToView(View view, object value)
        {
            var visibility = (bool)value;
            view.Visibility = visibility ? ViewStates.Visible : ViewStates.Gone;
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