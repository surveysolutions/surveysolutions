using System;
using System.Linq;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
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

            this.ViewModel.Items.OfType<CommentsViewModel>()
                .ForEach(x => x.CommentsInputShown += this.OnCommentsBlockShown());

            if (ViewModel?.ScrollToIndex != null)
            {
                this.layoutManager?.ScrollToPositionWithOffset(this.ViewModel.ScrollToIndex.Value, 0);
                this.ViewModel.ScrollToIndex = null;
            }
        }

        private EventHandler<EventArgs> OnCommentsBlockShown()
        {
            return (sender, args) =>
            {
                var firstVisibleItemPosition = this.layoutManager.FindFirstVisibleItemPosition();
                var lastVisibleItemPosition = this.layoutManager.FindLastVisibleItemPosition();

                var itemIndex = this.ViewModel.Items.ToList().IndexOf(sender as ICompositeEntity);

                if (itemIndex < firstVisibleItemPosition || itemIndex > lastVisibleItemPosition)
                {
                    this.layoutManager?.ScrollToPositionWithOffset(itemIndex, 200);
                    InputMethodManager imm = (InputMethodManager) this.Activity.GetSystemService(Android.Content.Context.InputMethodService);
                    imm.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.None);
                }
            };
        }
    }
}