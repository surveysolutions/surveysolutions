using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace WB.UI.Supervisor
{
    [Application]
    public class SupervisorApplication : MvxAppCompatApplication<SupervisorSetup, SupervisorMvxApplication>
    {
        public SupervisorApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
