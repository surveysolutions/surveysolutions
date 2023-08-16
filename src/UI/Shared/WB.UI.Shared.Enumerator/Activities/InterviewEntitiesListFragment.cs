﻿using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.InterviewEntitiesListFragment")]
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
            recyclerView?.SetItemAnimator(null);

            this.adapter = new InterviewEntityAdapter((IMvxAndroidBindingContext)this.BindingContext);
            //this.adapter.HasStableIds = true;
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
            
            Java.Interop.JniRuntime.CurrentRuntime.ValueManager.CollectPeers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            
            //GC.WaitForPendingFinalizers();
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

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
        public override void OnDetach()
        {
            base.OnDetach();
            
            recyclerView?.Dispose();
            layoutManager?.Dispose();
            adapter?.Dispose();
            
            recyclerView = null;
            layoutManager  = null;
            adapter = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                recyclerView?.Dispose();
                layoutManager?.Dispose();
                adapter?.Dispose();
            
                recyclerView = null;
                layoutManager  = null;
                adapter = null;
            }
            
            base.Dispose(disposing);
        }
    }
}
