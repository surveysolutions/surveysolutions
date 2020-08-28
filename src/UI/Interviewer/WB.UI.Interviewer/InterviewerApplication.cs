using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Interviewer
{
    [Application(UsesCleartextTraffic = true)]
    public class InterviewerApplication : MvxAndroidApplication<Setup, InterviewerMvxApplication>
    {
        public InterviewerApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
