using Android.Runtime;
using AndroidX.Work;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Supervisor
{
    [Application(UsesCleartextTraffic = true)]
    public class SupervisorApplication : MvxAndroidApplication<Setup, SupervisorMvxApplication>
    {
        public SupervisorApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            if (!WorkManager.IsInitialized)
            {
                var config = new Configuration.Builder()
                    .Build();
                WorkManager.Initialize(this, config);
            }
        }
    }
}
