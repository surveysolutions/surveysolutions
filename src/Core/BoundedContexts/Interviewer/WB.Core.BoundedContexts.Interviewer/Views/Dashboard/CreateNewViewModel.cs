using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CreateNewViewModel : ListViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IPlainStorage<AssignmentDocument, int> assignmentViewRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private SynchronizationViewModel synchronization;

        public IMvxCommand SynchronizationCommand => new MvxCommand(this.RunSynchronization, () => !this.synchronization.IsSynchronizationInProgress);

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<AssignmentDocument, int> assignmentViewRepository,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
            this.assignmentViewRepository = assignmentViewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public void Load(SynchronizationViewModel sync)
        {
            this.synchronization = sync;
            this.Title = InterviewerUIResources.Dashboard_AssignmentsTabTitle;

            var censusQuestionnairesCount = this.questionnaireViewRepository.Count(questionnaire => questionnaire.Census);
            var assignmentsCount = this.assignmentViewRepository.Count();

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
                var dashboardItem = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
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
                var assignment = this.UiItems.OfType<AssignmentDashboardItemViewModel>()
                    .FirstOrDefault(x => x.AssignmentId == assignmentId.Value);

                assignment?.DecreaseInterviewsCount();
            }
        }
    }
}
