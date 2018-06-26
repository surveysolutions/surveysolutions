using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ToBeAssignedItemsViewModel : ListViewModel
    {
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IAssignmentDocumentsStorage assignments;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;

        public ToBeAssignedItemsViewModel(IPlainStorage<InterviewView> interviews, 
            IAssignmentDocumentsStorage assignments,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal, 
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo)
        {
            this.interviews = interviews;
            this.assignments = assignments;
            this.viewModelFactory = viewModelFactory;
            this.principal = principal;
            this.identifyingQuestionsRepo = identifyingQuestionsRepo;
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
            var assignmentsToAssign = this.assignments.LoadAll();
            var interviewsToAssign = this.interviews.Where(x => x.ResponsibleId == this.principal.CurrentUserIdentity.UserId);

            foreach (var interviewView in interviewsToAssign)
            {
                var preffilledQuestions = this.identifyingQuestionsRepo
                    .Where(x => x.InterviewId == interviewView.InterviewId)
                    .OrderBy(x => x.SortIndex)
                    .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
                    .ToList();

                var dashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                dashboardItem.Init(interviewView, preffilledQuestions);

                yield return dashboardItem;
            }

            foreach (var assignment in assignmentsToAssign.Where(x => x.ResponsibleId == this.principal.CurrentUserIdentity.UserId))
            {
                var dashboardItem = this.viewModelFactory.GetNew<SupervisorAssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);

                yield return dashboardItem;
            }
        }
    }
}
