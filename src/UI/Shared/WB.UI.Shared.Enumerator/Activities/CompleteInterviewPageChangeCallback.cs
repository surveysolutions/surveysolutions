using System;
using Android.Runtime;
using AndroidX.ViewPager2.Widget;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewPageChangeCallback")]
    public class CompleteInterviewPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        private CompleteInterviewFragment fragment;

        public CompleteInterviewPageChangeCallback(CompleteInterviewFragment fragment)
        {
            this.fragment = fragment;
        }

        // Constructor for JNI (because [Register])
        protected CompleteInterviewPageChangeCallback(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnPageSelected(int position)
        {
            fragment?.OnPageChangedFromCallback();
        }

        protected override void Dispose(bool disposing)
        {
            this.fragment = null;
            base.Dispose(disposing);
        }
    }
}

