using System.Collections.Specialized;
using System.ComponentModel;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;

        private MvxRecyclerView recyclerView;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.tv_Complete_Groups);
            recyclerView.SetLayoutManager(new MvxGuardedLinearLayoutManager(Context));
            
            recyclerView.SetItemAnimator(null);
            
            ViewModel.CompleteGroups.CollectionChanged += AdjustRecyclerViewHeight;

            return view;
        }
        
        private int MeasureItemHeight(View view)
        {
            view.Measure(
                View.MeasureSpec.MakeMeasureSpec(recyclerView.Width, MeasureSpecMode.Exactly),
                View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            return view.MeasuredHeight;
        }

        private int CalculateTotalHeight()
        {
            int totalHeight = 0;
            var adapter = recyclerView.GetAdapter();

            if (adapter == null) return 0;

            for (int i = 0; i < adapter.ItemCount; i++)
            {
                int viewType = adapter.GetItemViewType(i);
                RecyclerView.ViewHolder viewHolder = (RecyclerView.ViewHolder)adapter.CreateViewHolder(recyclerView, viewType);
                adapter.BindViewHolder(viewHolder, i);
                totalHeight += MeasureItemHeight(viewHolder.ItemView);
            }

            totalHeight += recyclerView.PaddingTop + recyclerView.PaddingBottom;

            return totalHeight;
        }
        
        private void AdjustRecyclerViewHeight(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecalculateRecyclerViewHeight();
        }

        private void RecalculateRecyclerViewHeight()
        {
            if (recyclerView.Visibility != ViewStates.Visible)
                return;
            
            recyclerView.Post(() =>
            {
                var layoutParams = recyclerView.LayoutParameters;
                layoutParams.Height = CalculateTotalHeight();
                recyclerView.LayoutParameters = layoutParams;
            });
        }

        protected override void Dispose(bool disposing)
        {
            ViewModel.CompleteGroups.CollectionChanged -= AdjustRecyclerViewHeight;
            
            base.Dispose(disposing);
        }
    }
}
