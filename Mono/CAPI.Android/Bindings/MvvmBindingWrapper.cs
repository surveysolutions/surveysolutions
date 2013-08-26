using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace CAPI.Android.Bindings
{
    public abstract class MvvmBindingWrapper<T> : MvxAndroidTargetBinding where T: class 
    {
        protected MvvmBindingWrapper(T control)
            : base(control)
        {
        }

        private T View
        {
            get { return Target as T; }
        }

        public override void SetValue(object value)
        {
            if(View==null)
                return;
            SetValueToView(View, value);
        }

        protected abstract void SetValueToView(T view, object value);
    }
}