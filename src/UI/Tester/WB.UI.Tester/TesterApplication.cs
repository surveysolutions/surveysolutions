using System;
using Android.Runtime;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace WB.UI.Tester
{
    public class TesterApplication : MvxAppCompatApplication<TesterSetup, TesterMvxApplication>
    {
        public TesterApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
