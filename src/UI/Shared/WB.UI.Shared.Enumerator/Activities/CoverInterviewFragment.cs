using System;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Java.Lang;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;
using Object = Java.Lang.Object;

namespace WB.UI.Shared.Enumerator.Activities
{
    public class CoverInterviewFragment : BaseFragment<CoverInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_cover_screen;

        private class InnerRunnable : Object, IRunnable
        {
            private readonly Action action;

            public InnerRunnable(Action action)
            {
                this.action = action;
            }

            public void Run()
            {
                this.action.Invoke();
            }
        }
        
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (ViewModel?.ScrollToIdentity != null)
            {
                var nestedScrollView = view.FindViewById<NestedScrollView>(Resource.Id.tv_coverNestedScrollView);
                nestedScrollView.Post(new InnerRunnable(() =>
                {
                    var entityControl = nestedScrollView.FindViewWithTag($"tv_Title_{ViewModel.ScrollToIdentity}");
                    var topOffset = entityControl.Top + ((View)entityControl.Parent).Top;
                    nestedScrollView.ScrollTo(0, topOffset);
                    this.ViewModel.ScrollToIdentity = null;
                }));
            }
        }
    }
}
