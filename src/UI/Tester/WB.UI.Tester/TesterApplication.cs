using System;
using Android.Runtime;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Tester
{
    [Application(UsesCleartextTraffic = true)]
    public class TesterApplication : MvxAndroidApplication<Setup, TesterMvxApplication>
    {
        public TesterApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
