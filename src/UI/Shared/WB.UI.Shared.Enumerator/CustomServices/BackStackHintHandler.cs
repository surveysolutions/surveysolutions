using System;
using Android.Content;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class BackStackHintHandler
    {
        private readonly Context applicationcontext;
        private readonly Type mainActivityType;

        public BackStackHintHandler(Context applicationcontext, Type mainActivityType)
        {
            this.applicationcontext = applicationcontext;
            this.mainActivityType = mainActivityType;
        }

        public bool HandleClearBackstackHint(OpenLoginScreenHint openLoginScreenHint)
        {
            Intent i = new Intent(applicationcontext, mainActivityType);
            i.SetFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            applicationcontext.StartActivity(i); 

            return true;
        }
    }
}
