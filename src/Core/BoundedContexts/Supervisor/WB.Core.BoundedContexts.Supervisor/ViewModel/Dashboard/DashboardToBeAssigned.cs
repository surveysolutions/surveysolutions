using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ToBeAssignedItemsViewModel : ListViewModel
    {
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IAssignmentDocumentsStorage assignments;
        private readonly IInterviewViewModelFactory viewModelFactory;

        public ToBeAssignedItemsViewModel(IPlainStorage<InterviewView> interviews, 
            IAssignmentDocumentsStorage assignments,
            IInterviewViewModelFactory viewModelFactory)
        {
            this.interviews = interviews;
            this.assignments = assignments;
            this.viewModelFactory = viewModelFactory;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await this.UpdateUiItems();
        }

        public string TabTitle => SupervisorDashboard.ToBeAssigned;

        public override GroupStatus InterviewStatus => GroupStatus.NotStarted;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            var assignments = this.assignments.LoadAll();
            var interviews = this.interviews.LoadAll();

            if (assignments.Count > 0 || interviews.Count > 0)
            {
                var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
                subTitle.Title = InterviewerUIResources.Dashboard_CreateNewTabText;

                yield return subTitle;
            }

            foreach (var assignment in assignments)
            {
                var dashboardItem = this.viewModelFactory.GetNew<SupervisorAssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);

                yield return dashboardItem;
            }
        }
    }
}
