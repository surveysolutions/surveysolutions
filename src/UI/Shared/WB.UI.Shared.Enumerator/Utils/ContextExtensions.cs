using Android.App;
using Android.Views;

namespace WB.UI.Shared.Enumerator.Utils
{
    public static class ContextExtensions
    {
        public static Activity GetActivity(this View view)
        {
            return (Activity)view.Context;
        }
    }
}
