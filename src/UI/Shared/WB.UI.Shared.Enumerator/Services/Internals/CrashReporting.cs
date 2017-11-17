using System.Diagnostics;
using Android.Content;
using HockeyApp.Android;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    static class CrashReporting
    {
        public static void Init(Context context)
        {
            InitInternal(context);
        }

        [Conditional("RELEASE")]
        private static void InitInternal(Context context)
        {
            CrashManager.Register(context, new AutoSendingCrashListener());
        }
    }
}