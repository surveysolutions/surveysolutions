using System.Collections.Specialized;
using System.ComponentModel;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;
using Fragment = Android.App.Fragment;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteTabContentFragment : BaseFragment<TabViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete_tab_content;

        private MvxRecyclerView recyclerView;
        
        /*public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(ViewResourceId, container, false);
            return view;
        }*/
        
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            
            recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new MvxGuardedLinearLayoutManager(Context));
            recyclerView.SetItemAnimator(null);
        }
    }
}
