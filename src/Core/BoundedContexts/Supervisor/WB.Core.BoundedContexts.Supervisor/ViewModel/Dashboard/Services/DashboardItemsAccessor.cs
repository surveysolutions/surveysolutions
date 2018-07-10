using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services
{
    public class DashboardItemsAccessor : IDashboardItemsAccessor
    {
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IAssignmentDocumentsStorage assignments;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;
        private readonly IInterviewViewModelFactory viewModelFactory;

        public DashboardItemsAccessor(IPlainStorage<InterviewView> interviews, 
            IAssignmentDocumentsStorage assignments, 
            IPrincipal principal, 
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo, 
            IInterviewViewModelFactory viewModelFactory)
        {
            this.interviews = interviews;
            this.assignments = assignments;
            this.principal = principal;
            this.identifyingQuestionsRepo = identifyingQuestionsRepo;
            this.viewModelFactory = viewModelFactory;
        }

        public IEnumerable<IDashboardItem> TasksToBeAssigned()
        {
            var assignmentsToAssign = GetAssignmentsToAssign();
          
            foreach (var assignment in assignmentsToAssign)
            {
                var dashboardItem = this.viewModelFactory.GetNew<SupervisorAssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);

                yield return dashboardItem;
            }
        }

        public int TasksToBeAssignedCount()
        {
            return GetAssignmentsToAssign().Count();
        }

        public IEnumerable<IDashboardItem> WaitingForSupervisorAction()
        {
            var interviewsToAssign = GetItemsWaitingForSupervisorAction();

            foreach (var interviewView in interviewsToAssign)
            {
                var dashboardItem = ConvertInterviewToViewModel(interviewView);

                yield return dashboardItem;
            }
        }

        public int WaitingForSupervisorActionCount()
        {
            return GetItemsWaitingForSupervisorAction().Count;
        }

        public IEnumerable<IDashboardItem> Outbox()
        {
            var outboxAssignments = GetOutboxAssignments();

            var outboxInterviews = GetOutboxInterviews();

            foreach (var interviewView in outboxInterviews)
            {
                var dashboardItem = ConvertInterviewToViewModel(interviewView);

                yield return dashboardItem;
            }

            foreach (var assignment in outboxAssignments)
            {
                var dashboardItem = this.viewModelFactory.GetNew<SupervisorAssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);

                yield return dashboardItem;
            }
        }

        private SupervisorDashboardInterviewViewModel ConvertInterviewToViewModel(InterviewView interviewView)
        {
            var preffilledQuestions = this.identifyingQuestionsRepo
                .Where(x => x.InterviewId == interviewView.InterviewId)
                .OrderBy(x => x.SortIndex)
                .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
                .ToList();

            var dashboardItem = this.viewModelFactory.GetNew<SupervisorDashboardInterviewViewModel>();
            dashboardItem.Init(interviewView, preffilledQuestions);
            return dashboardItem;
        }

        private IReadOnlyCollection<InterviewView> GetOutboxInterviews()
        {
            return this.interviews.Where(x =>
                x.ResponsibleId != this.principal.CurrentUserIdentity.UserId
                || x.Status == InterviewStatus.ApprovedBySupervisor);
        }

        private IEnumerable<AssignmentDocument> GetOutboxAssignments()
        {
            return this.assignments.LoadAll()
                .Where(x => x.ResponsibleId != this.principal.CurrentUserIdentity.UserId && x.ReceivedByInterviewerAt == null);
        }

        public int OutboxCount()
        {
            return this.GetOutboxAssignments().Count() + this.GetOutboxInterviews().Count;
        }

        private IReadOnlyCollection<InterviewView> GetItemsWaitingForSupervisorAction()
        {
            return this.interviews.Where(x => x.ResponsibleId == this.principal.CurrentUserIdentity.UserId && 
                                              x.Status != InterviewStatus.ApprovedBySupervisor);
        }

        private IEnumerable<AssignmentDocument> GetAssignmentsToAssign()
        {
            return this.assignments.LoadAll().Where(x => x.ResponsibleId == this.principal.CurrentUserIdentity.UserId
                                                         && x.ReceivedByInterviewerAt == null);
        }
    }
}
