using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Supervisor
{
    [Application(UsesCleartextTraffic = true)]
    public class SupervisorApplication : MvxAndroidApplication<Setup, SupervisorMvxApplication>
    {
        public SupervisorApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
