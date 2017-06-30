using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using System;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CreateNewViewModel : ListViewModel<IDashboardItem>
    {
        public override int ItemsCount => this.UiItems.Count(
            item => item is CensusQuestionnaireDashboardItemViewModel || item is AssignmentDashboardItemViewModel);

        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewFromAssignmentCreatorService interviewFromAssignmentCreator;
        private SynchronizationViewModel Synchronization;

        private IMvxCommand synchronizationCommand;

        public IMvxCommand SynchronizationCommand => synchronizationCommand ??
                                                     (synchronizationCommand = new MvxCommand(this.RunSynchronization,
                                                         () => !this.Synchronization.IsSynchronizationInProgress));

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IViewModelNavigationService viewModelNavigationService,
            IInterviewFromAssignmentCreatorService interviewFromAssignmentCreator)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewFromAssignmentCreator = interviewFromAssignmentCreator;
        }

        public async Task LoadAsync(SynchronizationViewModel sync, EventHandler interviewCountChanged)
        {
            this.Synchronization = sync;
            this.Title = InterviewerUIResources.Dashboard_AssignmentsTabTitle;
            var assignments = await Task.Run(() => this.GetCensusQuestionnaires().Union(this.GetAssignments(interviewCountChanged)).ToList());

            var uiItems = assignments.Any()
                ? new List<IDashboardItem>(assignments.Count + 1) { this.GetSubTitle() }
                : new List<IDashboardItem>(assignments.Count);

            uiItems.AddRange(assignments);
            UiItems.ReplaceWith(uiItems);
        }

        private void RunSynchronization()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
                return;
            }

            this.Synchronization.IsSynchronizationInProgress = true;
            this.Synchronization.Synchronize();
        }

        private IDashboardItem GetSubTitle()
        {
            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_CreateNewTabText;
            return subTitle;
        }

        private IEnumerable<IDashboardItem> GetCensusQuestionnaires()
        {
            foreach (var censusQuestionnaireView in this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census))
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnaireView);

                yield return censusQuestionnaireDashboardItem;
            }
        }

        private IEnumerable<IDashboardItem> GetAssignments(EventHandler interviewCountChanged)
        {
            var interviewsCount = this.interviewViewRepository.LoadAll().ToLookup(iv => iv.Assignment);

            foreach (var assignment in this.assignmentsRepository.LoadAll())
            {
                var dashboardItem = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment, interviewCountChanged, this, interviewsCount[assignment.Id].Count());

                yield return dashboardItem;
            }
        }

        public int GetInterviewsCount(int assignmentId)
        {
            return this.interviewViewRepository.Count(iv => iv.Assignment == assignmentId);
        }

        public Task CreateInterviewAsync(int assignmentId)
        {
            return interviewFromAssignmentCreator.CreateInterviewAsync(assignmentId);
        }
    }
}