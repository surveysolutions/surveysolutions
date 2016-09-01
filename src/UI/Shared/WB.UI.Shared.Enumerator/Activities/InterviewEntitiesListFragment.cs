using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    public class InterviewEntitiesListFragment : BaseFragment<EnumerationStageViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_active_group;

        private MvxRecyclerView recyclerView;
        private LinearLayoutManager layoutManager;
        private InterviewEntityAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.EnsureBindingContextIsSet(savedInstanceState);
            var view = this.BindingInflate(ViewResourceId, container, false);
            this.recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.interviewEntitiesList);

            this.layoutManager = new LinearLayoutManager(this.Context);
            this.recyclerView.SetLayoutManager(this.layoutManager);

            this.adapter = new InterviewEntityAdapter((IMvxAndroidBindingContext)this.BindingContext);
            this.recyclerView.Adapter = this.adapter;

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (this.ViewModel != null && this.ViewModel.ScrollToIndex != null)
            {
                this.layoutManager?.ScrollToPositionWithOffset(this.ViewModel.ScrollToIndex.Value, 0);
                this.ViewModel.ScrollToIndex = null;
            }
        }
    }
}