using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace WB.UI.Interviewer
{
    [Application(UsesCleartextTraffic = true)]
    public class InterviewerApplication : MvxAppCompatApplication<Setup, InterviewerMvxApplication>
    {
        public InterviewerApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
