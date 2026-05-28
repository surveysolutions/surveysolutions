using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CreateNewViewModel : ListViewModel
    {
        public override DashboardGroupType DashboardType => DashboardGroupType.Assignments;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;
        private LocalSynchronizationViewModel synchronization = null!;

        public IMvxCommand SynchronizationCommand => new MvxCommand(this.RunSynchronization,
            () => !this.synchronization.IsSynchronizationInProgress && this.interviewerSettings.AllowSyncWithHq);

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IAssignmentDocumentsStorage assignmentsRepository,
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerSettings interviewerSettings)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
        }

        public async Task LoadAsync(LocalSynchronizationViewModel sync)
        {
            this.synchronization = sync;
            this.Title = EnumeratorUIResources.Dashboard_AssignmentsTabTitle;

            var censusQuestionnairesCount = this.questionnaireViewRepository.Count(questionnaire => questionnaire.Census);
            var assignmentsCount = this.assignmentsRepository.Count(a => a.Status == AssignmentStatus.Open);

            this.ItemsCount = censusQuestionnairesCount + assignmentsCount;

            await this.UpdateUiItemsAsync();
        }

        private void RunSynchronization()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
                return;
            }

            this.synchronization.IsSynchronizationInProgress = true;
            this.synchronization.Synchronize();
        }

        public bool SynchronizationWithHqEnabled => this.interviewerSettings.AllowSyncWithHq;

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            var dbQuestionnaires = this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census);
            var dbAssignments = this.assignmentsRepository.LoadAll().SortAssignments();

            if (dbQuestionnaires.Count > 0 || dbAssignments.Count > 0)
            {
                var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
                subTitle.Title = EnumeratorUIResources.Dashboard_CreateNewTabText;

                yield return subTitle;
            }

            foreach (var assignment in dbAssignments)
            {
                // Completed and Closed assignments are shown in the Completed tab
                if (assignment.Status == AssignmentStatus.Completed)
                    continue;

                // Approved assignments: hide from interviewer (no actions available)
                if (assignment.Status == AssignmentStatus.Closed)
                    continue;

                var dashboardItem = this.viewModelFactory.GetNew<InterviewerAssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);

                // Open assignments: show if unlimited or interviews still needed
                if (!dashboardItem.Quantity.HasValue || dashboardItem.InterviewsLeftByAssignmentCount > 0)
                {
                    yield return dashboardItem;
                }
            }
        }

        protected override IDashboardItemWithEvents GetUpdatedDashboardItem(IDashboardItemWithEvents dashboardItem)
        {
            var assignmentModel = (AssignmentDashboardItemViewModel)dashboardItem;
            var newAssignmentModel = this.viewModelFactory.GetNew<InterviewerAssignmentDashboardItemViewModel>();
            var assignment = this.assignmentsRepository.GetById(assignmentModel.AssignmentId);
            newAssignmentModel.Init(assignment);
            return newAssignmentModel;
        }

        public void UpdateAssignment(int? assignmentId)
        {
            if (assignmentId == null) return; 

            this.assignmentsRepository.DecreaseInterviewsCount(assignmentId.Value);

            this.UiItems
                .OfType<InterviewerAssignmentDashboardItemViewModel>()
                .FirstOrDefault(x => x.AssignmentId == assignmentId.Value)
                ?.DecreaseInterviewsCount();
        }

        /// <summary>
        /// Raised when an assignment in this tab changes status and should move to another tab.
        /// </summary>
        public event EventHandler? OnAssignmentStatusChanged;

        protected override void ListViewModel_OnItemUpdated(object sender, EventArgs args)
        {
            if (sender is AssignmentDashboardItemViewModel assignmentItem)
            {
                var assignment = assignmentsRepository.GetById(assignmentItem.AssignmentId);
                if (assignment == null || assignment.Status != AssignmentStatus.Open)
                {
                    // Assignment is no longer Open — remove it from this tab
                    assignmentItem.OnItemUpdated -= ListViewModel_OnItemUpdated;
                    UiItems.Remove(assignmentItem);
                    ItemsCount = Math.Max(0, ItemsCount - 1);
                    OnAssignmentStatusChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            base.ListViewModel_OnItemUpdated(sender, args);
        }
    }
}
