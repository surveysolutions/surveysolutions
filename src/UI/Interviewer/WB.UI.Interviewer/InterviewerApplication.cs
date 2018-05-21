using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace WB.UI.Interviewer
{
    [Application]
    public class InterviewerApplication : MvxAppCompatApplication<InterviewerSetup, InterviewerMvxApplication>
    {
        public InterviewerApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}
