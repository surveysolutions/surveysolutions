using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace WB.UI.Supervisor
{
    [Application(UsesCleartextTraffic = true)]
    public class SupervisorApplication : MvxAppCompatApplication<Setup, SupervisorMvxApplication>
    {
        public SupervisorApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
