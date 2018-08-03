using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.SharedKernels.DataCollection.Events;
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
                x.ReceivedByInterviewerAtUtc == null && (
                x.Status == InterviewStatus.ApprovedBySupervisor
                || x.ResponsibleId != this.principal.CurrentUserIdentity.UserId && 
                (x.Status == InterviewStatus.RejectedBySupervisor || 
                 x.Status == InterviewStatus.InterviewerAssigned)));
        }

        private IEnumerable<AssignmentDocument> GetOutboxAssignments()
        {
            return this.assignments.LoadAll()
                .Where(x => x.ResponsibleId != this.principal.CurrentUserIdentity.UserId &&
                            x.ReceivedByInterviewerAt == null);
        }

        public int OutboxCount()
        {
            return this.GetOutboxAssignments().Count() + this.GetOutboxInterviews().Count;
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
            return GetSentToInterviewerAssignments().Count() + GetSentToInterviewerInterviews().Count();
        }

        private IEnumerable<AssignmentDocument> GetSentToInterviewerAssignments()
        {
            return this.assignments.LoadAll().Where(x => x.ReceivedByInterviewerAt != null);
        }

        private IEnumerable<InterviewView> GetSentToInterviewerInterviews()
        {
            var sent = this.interviews.Where(x => x.ReceivedByInterviewerAtUtc != null);
            return sent;
        }

        private IReadOnlyCollection<InterviewView> GetItemsWaitingForSupervisorAction()
        {
            var itemsWaitingForSupervisorAction = this.interviews.Where(x => 
                x.ReceivedByInterviewerAtUtc == null && (
                x.Status == InterviewStatus.Completed || x.Status == InterviewStatus.RejectedByHeadquarters ||
                (x.Status == InterviewStatus.RejectedBySupervisor || 
                 x.Status == InterviewStatus.InterviewerAssigned || 
                 x.Status == InterviewStatus.SupervisorAssigned 
                 ) && x.ResponsibleId == this.principal.CurrentUserIdentity.UserId));
            return itemsWaitingForSupervisorAction;
        }

        private IEnumerable<AssignmentDocument> GetAssignmentsToAssign()
        {
            return this.assignments.LoadAll().Where(x => x.ResponsibleId == this.principal.CurrentUserIdentity.UserId
                                                         && x.ReceivedByInterviewerAt == null);
        }

        public bool IsWaitingForSupervisorActionInterview(Guid interviewId)
            => this.GetItemsWaitingForSupervisorAction().Any(x => x.InterviewId == interviewId);

        public bool IsOutboxInterview(Guid interviewId)
            => GetOutboxInterviews().Any(x => x.InterviewId == interviewId);

        public bool IsSentToInterviewer(Guid interviewId)
        {
            var sentToInterviewerInterviews = this.GetSentToInterviewerInterviews();

            return sentToInterviewerInterviews.Any(x => x.InterviewId == interviewId);
        }
    }
}
