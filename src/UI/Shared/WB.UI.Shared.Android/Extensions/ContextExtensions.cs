using Android.Content;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace WB.UI.Shared.Android.Extensions
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