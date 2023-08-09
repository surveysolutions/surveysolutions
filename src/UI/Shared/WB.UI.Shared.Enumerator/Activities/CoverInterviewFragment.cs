#nullable enable
using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.Widget;
using Java.Lang;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CoverInterviewFragment")]
    public class CoverInterviewFragment : BaseFragment<CoverInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_cover_screen;

        private class InnerRunnable :  Java.Lang.Object, IRunnable
        {
            private readonly WeakReference<Action> action;

            public InnerRunnable(Action action)
            {
                this.action = new WeakReference<Action>(action);
            }

            public void Run()
            {
                if (this.action.TryGetTarget(out var target))
                    target.Invoke();
            }
        }
        
        public override void OnViewCreated(View view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (ViewModel?.ScrollToIdentity == null) return;
            
            var nestedScrollView = view.FindViewById<NestedScrollView>(Resource.Id.tv_coverNestedScrollView);
            nestedScrollView?.Post(new InnerRunnable(() =>
            {
                var entityControl = nestedScrollView.FindViewWithTag($"tv_Title_{ViewModel.ScrollToIdentity}");
                if (entityControl == null) return;
                
                if (entityControl.Parent is View parent)
                {
                    {
                        var topOffset = entityControl.Top + parent.Top;
                        nestedScrollView.ScrollTo(0, topOffset);
                    }
                }

                this.ViewModel.ScrollToIdentity = null;
            }));
        }
    }
}
