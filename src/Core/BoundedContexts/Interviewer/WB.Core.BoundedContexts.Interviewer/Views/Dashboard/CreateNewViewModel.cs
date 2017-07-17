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
    public class CreateNewViewModel : ListViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private SynchronizationViewModel synchronization;

        private IReadOnlyCollection<QuestionnaireView> dbQuestionnaires;
        private IReadOnlyCollection<AssignmentDocument> dbAssignments;

        private IMvxCommand synchronizationCommand;

        public IMvxCommand SynchronizationCommand => synchronizationCommand ??
                                                     (synchronizationCommand = new MvxCommand(this.RunSynchronization,
                                                         () => !this.synchronization.IsSynchronizationInProgress));

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public async Task Load(SynchronizationViewModel sync)
        {
            this.synchronization = sync;
            this.Title = InterviewerUIResources.Dashboard_AssignmentsTabTitle;

            this.dbQuestionnaires = this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census);
            this.dbAssignments = this.assignmentsRepository.LoadAll();

            this.ItemsCount = this.dbQuestionnaires.Count + this.dbAssignments.Count;

            var uiItems = await Task.Run(() => this.GetUiItems());
            this.UiItems.ReplaceWith(uiItems);
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

        private IEnumerable<IDashboardItem> GetUiItems()
        {
            if (this.dbQuestionnaires.Count > 0 || this.dbAssignments.Count > 0)
            {
                var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
                subTitle.Title = InterviewerUIResources.Dashboard_CreateNewTabText;

                yield return subTitle;
            }

            foreach (var censusQuestionnaireView in this.dbQuestionnaires)
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnaireView);

                yield return censusQuestionnaireDashboardItem;
            }

            var interviewsCount = this.interviewViewRepository.LoadAll().ToLookup(iv => iv.Assignment);

            foreach (var assignment in this.dbAssignments)
            {
                var dashboardItem = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment, interviewsCount[assignment.Id].Count());

                yield return dashboardItem;
            }
        }

        public void UpdateAssignment(int? assignmentId)
        {
            if (!assignmentId.HasValue) return;

            var assignment = this.UiItems.OfType<AssignmentDashboardItemViewModel>()
                .FirstOrDefault(x => x.AssignmentId == assignmentId.Value);

            assignment?.DecreaseInterviewsCount();
        }
    }
}