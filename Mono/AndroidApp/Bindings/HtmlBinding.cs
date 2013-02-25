using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.Binding.Interfaces;
namespace AndroidApp.Bindings
{
    public class HtmlBinding : MvxBaseAndroidTargetBinding
    {
        private readonly TextView _control;

        public HtmlBinding(TextView control)
        {
            _control = control;
        }
        public override void SetValue(object value)
        {
            var htmlString = (string) value;
            _control.SetText(Html.FromHtml(htmlString), TextView.BufferType.Spannable);
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