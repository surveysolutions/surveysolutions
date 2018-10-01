using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class WaitingForSupervisorActionViewModel : RefreshingAfterSyncListViewModel, IMvxViewModel<Guid?>
    {
        private readonly IDashboardItemsAccessor dashboardItemsAccessor;
        private readonly IInterviewViewModelFactory viewModelFactory;

        private int? highLightedItemIndex;
        public int? HighLightedItemIndex
        {
            get => highLightedItemIndex;
            set => SetProperty(ref highLightedItemIndex, value);
        }

        private Guid? lastVisitedInterviewId;

        public void Prepare(Guid? parameter) => this.lastVisitedInterviewId = parameter;

        public WaitingForSupervisorActionViewModel(IDashboardItemsAccessor dashboardItemsAccessor,
            IInterviewViewModelFactory viewModelFactory)
        {
            this.dashboardItemsAccessor = dashboardItemsAccessor;
            this.viewModelFactory = viewModelFactory;

            this.Title = SupervisorDashboard.WaitingForAction;
        }

        public override GroupStatus InterviewStatus => GroupStatus.Started;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            var subtitle = viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subtitle.Title = SupervisorDashboard.WaitingForSupervisorActionSubtitle;

            yield return subtitle;

            var interviewIndex = 1;
            foreach (var dashboardItem in this.dashboardItemsAccessor.WaitingForSupervisorAction())
            {
                yield return dashboardItem;

                if (dashboardItem is SupervisorDashboardInterviewViewModel interviewDashboardItem &&
                    interviewDashboardItem.InterviewId == lastVisitedInterviewId)
                {
                    this.HighLightedItemIndex = interviewIndex;
                }

                interviewIndex++;
            }
        }
    }
}
