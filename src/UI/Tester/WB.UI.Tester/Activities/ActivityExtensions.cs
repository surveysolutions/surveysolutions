using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace WB.UI.QuestionnaireTester.Activities
{
    public static class ActivityExtensions
    {
        public static void RemoveFocusFromEditText(this Activity activity)
        {
            View viewWithFocus = activity.CurrentFocus;

            if (viewWithFocus is EditText)
            {
                viewWithFocus.ClearFocus();
            }
        }

        public static void HideKeyboard(this Activity activity, IBinder windowToken)
        {
            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            inputMethodManager.HideSoftInputFromWindow(windowToken, 0);
        }     
    }
}