using System;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
namespace CAPI.Android.Bindings
{
    public class HtmlBinding : MvxAndroidTargetBinding
    {
        private readonly TextView control;

        public HtmlBinding(TextView control):base(control)
        {
            this.control = control;
        }
        public override void SetValue(object value)
        {
            var htmlString = (string) value;
            control.SetText(Html.FromHtml(htmlString), TextView.BufferType.Spannable);
        }

        public override Type TargetType
        {
            get { return typeof(string); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}