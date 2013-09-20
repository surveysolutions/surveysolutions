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
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace CAPI.Android.Extensions
{
    public static class ContextExtensions
    {
        public static IMvxAndroidBindingContext ToBindingContext(this Context context)
        {
            return
                (context as IMvxBindingContextOwner).BindingContext as IMvxAndroidBindingContext;
        }
    }
}