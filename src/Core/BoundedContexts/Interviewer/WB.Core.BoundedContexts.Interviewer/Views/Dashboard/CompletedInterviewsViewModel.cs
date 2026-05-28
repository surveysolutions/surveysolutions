using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CompletedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override DashboardGroupType DashboardType => DashboardGroupType.CompletedInterviews;
        public override string TabTitle => EnumeratorUIResources.Dashboard_CompletedLinkText;
        public override string TabDescription => EnumeratorUIResources.Dashboard_CompletedTabText;
        
        public event EventHandler<InterviewRemovedArgs>? OnInterviewRemoved;

        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IInterviewViewModelFactory assignmentViewModelFactory;

        public CompletedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal,
            IAssignmentDocumentsStorage assignmentsRepository) 
            : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo, principal)
        {
            this.assignmentsRepository = assignmentsRepository;
            this.assignmentViewModelFactory = viewModelFactory;
        }

        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = Principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                (interview.Mode == InterviewMode.CAPI || interview.Mode == null) && 
                                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview
                                    .InterviewStatus.Completed;
        }

        protected override void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem)
        {
            base.OnItemCreated(interviewDashboardItem);
            interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;
        }

        public override async System.Threading.Tasks.Task LoadAsync(Guid? lastVisitedInterviewId)
        {
            await base.LoadAsync(lastVisitedInterviewId);

            if (Principal.IsAuthenticated)
            {
                var completedAssignmentsCount = assignmentsRepository.Count(a => a.Status == AssignmentStatus.Completed);
                this.ItemsCount += completedAssignmentsCount;
                this.UpdateTitle();
            }
        }

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            // First yield all completed interviews from the base class
            foreach (var item in base.GetUiItems())
                yield return item;

            // Then yield completed assignments
            if (!Principal.IsAuthenticated) yield break;

            var completedAssignments = assignmentsRepository.Query(a => a.Status == AssignmentStatus.Completed);
            foreach (var assignment in completedAssignments)
            {
                yield return assignmentViewModelFactory.GetDashboardAssignment(assignment);
            }
        }

        protected override IDashboardItemWithEvents GetUpdatedDashboardItem(IDashboardItemWithEvents dashboardItemWithEvents)
        {
            if (dashboardItemWithEvents is AssignmentDashboardItemViewModel assignmentItem)
            {
                var assignment = assignmentsRepository.GetById(assignmentItem.AssignmentId);
                return (IDashboardItemWithEvents)assignmentViewModelFactory.GetDashboardAssignment(assignment);
            }

            return base.GetUpdatedDashboardItem(dashboardItemWithEvents);
        }

        protected override void ListViewModel_OnItemUpdated(object? sender, EventArgs args)
        {
            if (sender is AssignmentDashboardItemViewModel assignmentItem)
            {
                var assignment = assignmentsRepository.GetById(assignmentItem.AssignmentId);
                if (assignment?.Status != AssignmentStatus.Completed)
                {
                    // Assignment is no longer Completed — remove it from this tab
                    assignmentItem.OnItemUpdated -= ListViewModel_OnItemUpdated;
                    UiItems.Remove(assignmentItem);
                    ItemsCount--;
                    UpdateTitle();
                    OnAssignmentStatusChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            base.ListViewModel_OnItemUpdated(sender, args);
        }

        /// <summary>
        /// Raised when an assignment in this tab changes status and should move to another tab.
        /// </summary>
        public event EventHandler? OnAssignmentStatusChanged;

        private async void InterviewDashboardItem_OnItemRemoved(object? sender, System.EventArgs e)
        {
            if (sender == null) return;
            var dashboardItem = (InterviewDashboardItemViewModel) sender;

            this.ItemsCount--;
            this.UpdateTitle();

            await this.InvokeOnMainThreadAsync(() =>
            {
                this.UiItems.Remove(dashboardItem);

            }, false).ConfigureAwait(false);

            this.OnInterviewRemoved?.Invoke(sender,
                new InterviewRemovedArgs(dashboardItem.AssignmentId, dashboardItem.InterviewId));
        }
    }
}
