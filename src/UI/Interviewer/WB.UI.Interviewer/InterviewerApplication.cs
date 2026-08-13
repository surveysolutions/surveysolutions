using Android.Runtime;
using AndroidX.Work;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Interviewer
{
    [Application(UsesCleartextTraffic = true)]
    public class InterviewerApplication : MvxAndroidApplication<Setup, InterviewerMvxApplication>
    {
        public InterviewerApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            // WorkManager auto-initialization via androidx.startup.InitializationProvider
            // is not present in the merged manifest. Initialize it manually.
            if (!WorkManager.IsInitialized)
            {
                var config = new Configuration.Builder()
                    .Build();
                WorkManager.Initialize(this, config);
            }
        }
    }
}
