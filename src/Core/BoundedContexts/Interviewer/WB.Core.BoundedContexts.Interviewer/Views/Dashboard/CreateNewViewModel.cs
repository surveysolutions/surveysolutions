using System;
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

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CreateNewViewModel : ListViewModel<IDashboardItem>
    {
        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;

        private SynchronizationViewModel Synchronization;

        private IMvxCommand synchronizationCommand;
        public IMvxCommand SynchronizationCommand => synchronizationCommand ??
                                                     (synchronizationCommand = new MvxCommand(this.RunSynchronization, () => !this.Synchronization.IsSynchronizationInProgress));

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IAssignmentDocumentsStorage assignmentsRepository,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public void Load(SynchronizationViewModel sync, DashboardViewModel dashboardViewModel)
        {
            this.Synchronization = sync;

            LoadAsync(dashboardViewModel);

            this.Title = InterviewerUIResources.Dashboard_AssignmentsTabTitle;
        }

        private async void LoadAsync(DashboardViewModel dashboardViewModel)
        {
            dashboardViewModel.IsInProgress = true;

            this.Items = this.Items ?? new List<IDashboardItem>();
            this.UiItems = this.UiItems ?? new List<IDashboardItem>();

            var assignments = await Task.Run(() => this.GetCensusQuestionnaires().Union(this.GetAssignments(dashboardViewModel)).ToList());

            var uiItems = assignments.Any()
                ? new List<IDashboardItem>(assignments.Count + 1) {this.GetSubTitle()}
                : new List<IDashboardItem>(assignments.Count);

            uiItems.AddRange(assignments);

            Items = assignments;
            UiItems = uiItems;

            dashboardViewModel.IsInProgress = false;
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

        private IEnumerable<IDashboardItem> GetAssignments(DashboardViewModel dashboardViewModel)
        {
            foreach (var assignment in this.assignmentsRepository.LoadAll())
            {
                var dashboardItem = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment, dashboardViewModel);

                yield return dashboardItem;
            }
        }
    }
}