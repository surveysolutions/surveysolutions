using System;
using Android.OS;
using Android.Views;
using AndroidX.Core.Widget;
using Java.Lang;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
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
                    var topOffset = entityControl != null
                        ? entityControl.Top + ((View)entityControl.Parent).Top
                        : 0;
                    nestedScrollView.ScrollTo(0, topOffset);
                    this.ViewModel.ScrollToIdentity = null;
                }));
            }
        }
    }
}
