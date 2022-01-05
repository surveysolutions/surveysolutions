using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.OverviewFragment")]
    public class OverviewFragment : BaseFragment<OverviewViewModel>
    {
        private MvxRecyclerView recyclerView;

        protected override int ViewResourceId => Resource.Layout.interview_overview;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.EnsureBindingContextIsSet(inflater);
            var view = this.BindingInflate(ViewResourceId, container, false);
            this.recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.overview_recycler);

            this.recyclerView.SetLayoutManager(new LinearLayoutManager(this.Context));
            this.recyclerView.Adapter.ItemClick = new MvxAsyncCommand<OverviewNode>(async node =>
            {
                var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
                await viewModelNavigationService.NavigateToAsync<OverviewNodeDetailsViewModel, OverviewNodeDetailsViewModelArgs>(
                    new OverviewNodeDetailsViewModelArgs
                    {
                        InterviewId = ViewModel.InterviewId,
                        TargetEntity = Identity.Parse(node.Id)
                    });
            }, node =>
            {
                if (node is OverviewQuestionViewModel question)
                {
                    return question.HasComment || question.HasErrors || question.HasWarnings;
                }
                else if (node is OverviewStaticText staticText)
                {
                    return staticText.HasErrors || staticText.HasWarnings;
                }

                return false;
            });

            return view;
        }

    }
}
