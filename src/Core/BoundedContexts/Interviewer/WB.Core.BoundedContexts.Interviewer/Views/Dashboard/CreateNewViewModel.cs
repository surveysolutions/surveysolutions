using System.Collections.Generic;
using System.Linq;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CreateNewViewModel : ListViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;
        private SynchronizationViewModel synchronization;

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

        public void Load(SynchronizationViewModel sync)
        {
            this.synchronization = sync;
            this.Title = InterviewerUIResources.Dashboard_AssignmentsTabTitle;

            var censusQuestionnairesCount = this.questionnaireViewRepository.Count(questionnaire => questionnaire.Census);
            var assignmentsCount = this.assignmentsRepository.Count();

            this.ItemsCount = censusQuestionnairesCount + assignmentsCount;

            this.UpdateUiItems();
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
            var dbAssignments = this.assignmentsRepository.LoadAll();

            if (dbQuestionnaires.Count > 0 || dbAssignments.Count > 0)
            {
                var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
                subTitle.Title = InterviewerUIResources.Dashboard_CreateNewTabText;

                yield return subTitle;
            }

            foreach (var censusQuestionnaireView in dbQuestionnaires)
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnaireView);

                yield return censusQuestionnaireDashboardItem;
            }

            foreach (var assignment in dbAssignments)
            {
                var dashboardItem = this.viewModelFactory.GetNew<InterviewerAssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);

                yield return dashboardItem;
            }
        }

        public void UpdateAssignment(int? assignmentId)
        {
            if (!assignmentId.HasValue)
            {
                this.UiItems.OfType<CensusQuestionnaireDashboardItemViewModel>()
                    .ForEach(x => x.UpdateSubtitle());
            }
            else
            {
                // update UI assignment
                var assignment = this.UiItems.OfType<InterviewerAssignmentDashboardItemViewModel>()
                    .FirstOrDefault(x => x.AssignmentId == assignmentId.Value);

                assignment?.DecreaseInterviewsCount();
            }
        }
    }
}
