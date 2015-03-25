using System;
using Android.Views;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{
    public class VisibleBinding : MvvmBindingWrapper<View>
    {
        public VisibleBinding(View control):base(control)
        {
        }

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
    }
}