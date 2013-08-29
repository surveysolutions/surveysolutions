using System;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
namespace CAPI.Android.Bindings
{
    public class HtmlBinding : MvvmBindingWrapper<TextView>
    {
        public HtmlBinding(TextView control):base(control)
        {
        }

        protected override void SetValueToView(TextView view, object value)
        {
            var htmlString = (string)value;
            view.SetText(Html.FromHtml(htmlString), TextView.BufferType.Spannable);
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