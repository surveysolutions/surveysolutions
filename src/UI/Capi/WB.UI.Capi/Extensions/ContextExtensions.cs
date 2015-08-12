using Android.App;
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


        public static void ClearAllBackStack<T>(this Context context) where T : Activity
        {
            Intent intent = new Intent(context, typeof(T));
            intent.PutExtra("finish", true); // if you are checking for this in your other Activities
            intent.AddFlags(ActivityFlags.ClearTask);
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
    }
}