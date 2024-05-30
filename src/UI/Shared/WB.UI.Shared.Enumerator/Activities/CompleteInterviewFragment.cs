using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.tv_Complete_Groups);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));
            
            recyclerView.SetItemAnimator(new NoAnimationItemAnimator());

            return view;
        }
    }
}
