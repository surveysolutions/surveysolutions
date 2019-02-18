using Android.App;
using Android.Content;
using Android.Views;

namespace WB.UI.Shared.Enumerator.Utils
{
    public static class ContextExtensions
    {
        public static Activity GetActivity(this View view)
        {
            Context context = view.Context;
            while (context is ContextWrapper wrapper)
            {
                if (wrapper is Activity activity)
                {
                    return activity;
                }
                context = wrapper.BaseContext;
            }
            return null;
        }
    }
}
