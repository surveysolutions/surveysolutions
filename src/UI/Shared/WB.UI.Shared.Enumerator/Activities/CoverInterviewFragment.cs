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
        private MvxRecyclerView recyclerView;
        private LinearLayoutManager layoutManager;
        private InterviewEntityAdapter adapter;

        /*public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.EnsureBindingContextIsSet(inflater);
            var view = this.BindingInflate(ViewResourceId, container, false);
            this.recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.interviewEntitiesList);

            this.layoutManager = new LinearLayoutManager(this.Context);
            this.recyclerView.SetLayoutManager(this.layoutManager);

            this.adapter = new InterviewEntityAdapter((IMvxAndroidBindingContext)this.BindingContext);
            this.recyclerView.Adapter = this.adapter;

            return view;
        }*/

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (ViewModel?.ScrollToIndex != null)
            {
                // var childView = this.recyclerView;
                // int scrollTo =  childView.Top;

                var nestedScrollView = view.FindViewById<NestedScrollView>(Resource.Id.tv_coverNestedScrollView);
                nestedScrollView.Post(new InnerRunnable(() =>
                {
                    var entityControl = nestedScrollView.FindViewWithTag($"tv_Title_{ViewModel.ScrollToIdentity}");
                    var topOffset = entityControl.Top + entityControl.LayoutParameters.Height + ((View)entityControl.Parent).Top;
                    nestedScrollView.ScrollTo(0, topOffset);
                    //nestedScrollView.SmoothScrollTo(0, 0);
                    this.ViewModel.ScrollToIndex = null;
                    this.ViewModel.ScrollToIdentity = null;
                }));
                
                
                /*int scrollViewHeight = nestedScrollView.Height;
                if (scrollViewHeight > 0) {
                    nestedScrollView.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

                    View lastView = nestedScrollView.GetChildAt(nestedScrollView.ChildCount - 1);
                    int lastViewBottom = lastView.Bottom + nestedScrollView.PaddingBottom;
                    int deltaScrollY = lastViewBottom - scrollViewHeight - nestedScrollView.ScrollY;
                    /* If you want to see the scroll animation, call this. #1#
                    //nestedScrollView.SmoothScrollBy(0, deltaScrollY);
                    /* If you don't want, call this. #1#
                    nestedScrollView.ScrollBy(0, deltaScrollY);
                }*/
                    //nestedScrollView.fullScroll(View.FOCUS_DOWN);

                //nestedScrollView.ScrollTo(0, 1000);
                //this.layoutManager?.ScrollToPosition(this.ViewModel.ScrollToIndex.Value);
                //this.ViewModel.ScrollToIndex = null;
            }
        }
    }
}
