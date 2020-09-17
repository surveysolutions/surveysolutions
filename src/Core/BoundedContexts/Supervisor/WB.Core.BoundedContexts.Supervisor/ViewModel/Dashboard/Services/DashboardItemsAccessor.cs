using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                var dashboardItem = ConvertAssignmentToViewModel(assignment);
                yield return dashboardItem;
            }
        }

        public int TasksToBeAssignedCount()
        {
            return GetAssignmentsToAssignCount();
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
            return GetItemsWaitingForSupervisorActionCount();
        }

        public IEnumerable<IDashboardItem> Outbox()
        {
            var outboxInterviews = GetOutboxInterviews();
            foreach (var interviewView in outboxInterviews)
            {
                var dashboardItem = ConvertInterviewToViewModel(interviewView);
                yield return dashboardItem;
            }

            var outboxAssignments = GetOutboxAssignments();
            foreach (var assignment in outboxAssignments)
            {
                var dashboardItem = ConvertAssignmentToViewModel(assignment);
                yield return dashboardItem;
            }
        }

        private SupervisorDashboardInterviewViewModel ConvertInterviewToViewModel(InterviewView interviewView)
        {
            var prefilledQuestions = this.identifyingQuestionsRepo
                .Where(x => x.InterviewId == interviewView.InterviewId)
                .OrderBy(x => x.SortIndex)
                .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
                .ToList();

            var dashboardItem = this.viewModelFactory.GetNew<SupervisorDashboardInterviewViewModel>();
            dashboardItem.Init(interviewView, prefilledQuestions);
            return dashboardItem;
        }

        private SupervisorAssignmentDashboardItemViewModel ConvertAssignmentToViewModel(AssignmentDocument assignmentDocument)
        {
            var dashboardItem = this.viewModelFactory.GetNew<SupervisorAssignmentDashboardItemViewModel>();
            dashboardItem.Init(assignmentDocument);
            return dashboardItem;
        }

        private Expression<Func<InterviewView, bool>> GetOutboxInterviewsFilter()
        {
            var userId = this.principal.CurrentUserIdentity.UserId;

            return x =>
                x.ReceivedByInterviewerAtUtc == null && 
                (x.Status == InterviewStatus.ApprovedBySupervisor || 
                 x.ResponsibleId != userId && 
                 (x.Status == InterviewStatus.RejectedBySupervisor || 
                  x.Status == InterviewStatus.InterviewerAssigned || 
                  x.Status == InterviewStatus.Restarted));
        }

        private IReadOnlyCollection<InterviewView> GetOutboxInterviews()
        {
            return this.interviews.Where(GetOutboxInterviewsFilter());
        }

        private int GetOutboxInterviewsCount()
        {
            return this.interviews.Count(GetOutboxInterviewsFilter());
        }

        private Expression<Func<AssignmentDocument, bool>> GetOutboxAssignmentsFilter()
        {
            var userId = this.principal.CurrentUserIdentity.UserId;
            return x => 
                x.ResponsibleId != userId 
                && x.ReceivedByInterviewerAt == null;
        }

        private IEnumerable<AssignmentDocument> GetOutboxAssignments()
        {
            return this.assignments.Query(GetOutboxAssignmentsFilter());
        }

        private int GetOutboxAssignmentsCount()
        {
            return this.assignments.Count(GetOutboxAssignmentsFilter());
        }

        public int OutboxCount()
        {
            return this.GetOutboxAssignmentsCount() + this.GetOutboxInterviewsCount();
        }

        public IEnumerable<IDashboardItem> GetSentToInterviewerItems()
        {
            var outboxAssignments = GetSentToInterviewerAssignments();

            var outboxInterviews = GetSentToInterviewerInterviews();

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

        public int SentToInterviewerCount()
        {
            return this.assignments.Count(GetSentToInterviewerAssignmentsFilter()) 
                 + this.interviews.Count(GetSentToInterviewerInterviewsFilter());
        }

        private Expression<Func<AssignmentDocument, bool>> GetSentToInterviewerAssignmentsFilter()
        {
            return x => x.ReceivedByInterviewerAt != null;
        }

        private IEnumerable<AssignmentDocument> GetSentToInterviewerAssignments()
        {
            return this.assignments.Query(GetSentToInterviewerAssignmentsFilter());
        }

        private Expression<Func<InterviewView, bool>> GetSentToInterviewerInterviewsFilter()
        {
            return x => x.ReceivedByInterviewerAtUtc != null;
        }

        private IEnumerable<InterviewView> GetSentToInterviewerInterviews()
        {
            return this.interviews.Where(GetSentToInterviewerInterviewsFilter());
        }

        //usage of property causes an error
        // https://github.com/praeclarum/sqlite-net/issues/535
        private Expression<Func<InterviewView, bool>> GetItemsWaitingForSupervisorActionFilter()
        {
            var userId = this.principal.CurrentUserIdentity.UserId;

            return x => 
                x.ReceivedByInterviewerAtUtc == null && (
                    x.Status == InterviewStatus.Completed || x.Status == InterviewStatus.RejectedByHeadquarters ||
                    (x.Status == InterviewStatus.RejectedBySupervisor || 
                     x.Status == InterviewStatus.InterviewerAssigned || 
                     x.Status == InterviewStatus.SupervisorAssigned 
                    ) && x.ResponsibleId == userId);
        }

        private IReadOnlyCollection<InterviewView> GetItemsWaitingForSupervisorAction()
        {
            var itemsWaitingForSupervisorAction = 
                this.interviews.Where(GetItemsWaitingForSupervisorActionFilter());
            return itemsWaitingForSupervisorAction;
        }

        private int GetItemsWaitingForSupervisorActionCount()
        {
            var count = this.interviews.Count(GetItemsWaitingForSupervisorActionFilter());
            return count;
        }

        private Expression<Func<AssignmentDocument, bool>> GetAssignmentsToAssignFilter()
        {
            var userId = this.principal.CurrentUserIdentity.UserId;

            return x =>
                x.ResponsibleId == userId
                && x.ReceivedByInterviewerAt == null;
        }

        private IEnumerable<AssignmentDocument> GetAssignmentsToAssign()
        {
            return this.assignments.Query(GetAssignmentsToAssignFilter());
        }

        private int GetAssignmentsToAssignCount()
        {
            return this.assignments.Count(GetAssignmentsToAssignFilter());
        }

        public bool IsWaitingForSupervisorActionInterview(Guid interviewId)
            => this.GetItemsWaitingForSupervisorAction().Any(x => x.InterviewId == interviewId);

        public bool IsOutboxInterview(Guid interviewId)
            => GetOutboxInterviews().Any(x => x.InterviewId == interviewId);

        public bool IsSentToInterviewer(Guid interviewId) 
            => GetSentToInterviewerInterviews().Any(x => x.InterviewId == interviewId);
    }
}
