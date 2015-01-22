using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Util;

namespace WB.UI.QuestionnaireTester.Controls
{
    public class MvxSwipeRefreshLayout : SwipeRefreshLayout
    {
        protected MvxSwipeRefreshLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public MvxSwipeRefreshLayout(Context p0) : base(p0)
        {
        }

        public MvxSwipeRefreshLayout(Context p0, IAttributeSet p1) : base(p0, p1)
        {

        }
    }
}