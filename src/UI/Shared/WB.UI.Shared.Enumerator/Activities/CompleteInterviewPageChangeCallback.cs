using System;
using Android.Runtime;
using AndroidX.ViewPager2.Widget;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewPageChangeCallback")]
    public class CompleteInterviewPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        private WeakReference<CompleteInterviewFragment> fragment;

        public CompleteInterviewPageChangeCallback(CompleteInterviewFragment fragment)
        {
            this.fragment = new WeakReference<CompleteInterviewFragment>(fragment);
        }

        // Constructor for JNI (because [Register])
        protected CompleteInterviewPageChangeCallback(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnPageSelected(int position)
        {
            if (this.fragment.TryGetTarget(out var fragmentObj))
                fragmentObj?.OnPageChangedFromCallback();
        }

        protected override void Dispose(bool disposing)
        {
            this.fragment = null;
            base.Dispose(disposing);
        }
    }
}

