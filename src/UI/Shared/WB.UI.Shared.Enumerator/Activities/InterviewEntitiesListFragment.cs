using System;
using System.Linq;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
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
            this.EnsureBindingContextIsSet(inflater);
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

            this.ViewModel?.Items.OfType<CommentsViewModel>()
                .ForEach(x => x.CommentsInputShown += this.OnCommentsBlockShown);

            if (ViewModel?.ScrollToIndex != null)
            {
                this.layoutManager?.ScrollToPositionWithOffset(this.ViewModel.ScrollToIndex.Value, 0);
                this.ViewModel.ScrollToIndex = null;
            }
        }

        public override void OnDestroyView()
        {
            this.ViewModel?.Items.OfType<CommentsViewModel>()
                .ForEach(x => x.CommentsInputShown -= this.OnCommentsBlockShown);

            base.OnDestroyView();
        }

        private void OnCommentsBlockShown(object sender, EventArgs args)
        {
            var compositeEntities = this.ViewModel.Items.ToList();
            var itemIndex = compositeEntities.IndexOf(sender as ICompositeEntity);

            var isAtTheEndOfAList = itemIndex - compositeEntities.Count < 2;
            if (isAtTheEndOfAList || itemIndex < this.layoutManager.FindFirstVisibleItemPosition() || itemIndex > this.layoutManager.FindLastVisibleItemPosition())
            {
                InputMethodManager imm = (InputMethodManager) this.Activity.GetSystemService(Android.Content.Context.InputMethodService);
                imm.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.None);
                this.layoutManager?.ScrollToPositionWithOffset(itemIndex, 200);
            }
        }
    }
}
