using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CreateNewViewModel : ListViewModel<IDashboardItem>
    {
        public string Description => InterviewerUIResources.Dashboard_CreateNewTabText;
        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<AssignmentDocument> assignmentsRepository;

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<AssignmentDocument> assignmentsRepository)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
        }

        public void Load()
        {
            var listDashboardItems = new List<IDashboardItem>();
            listDashboardItems.AddRange(this.GetCensusQuestionnaires());
            listDashboardItems.AddRange(this.GetAssignments());

            this.Items = listDashboardItems;
            this.Title = InterviewerUIResources.Dashboard_CreateNewLinkText;
        }

        private List<CensusQuestionnaireDashboardItemViewModel> GetCensusQuestionnaires()
        {
            var censusQuestionnaireViews = this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census);

            // show census mode for new tab
            var censusQuestionnaireViewModels = new List<CensusQuestionnaireDashboardItemViewModel>();
            foreach (var censusQuestionnaireView in censusQuestionnaireViews)
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnaireView);
                censusQuestionnaireViewModels.Add(censusQuestionnaireDashboardItem);
            }

            return censusQuestionnaireViewModels;
        }

        private List<AssignmentDashboardItemViewModel> GetAssignments()
        {
            var assignments = this.assignmentsRepository.LoadAll();

            var dashboardItems = new List<AssignmentDashboardItemViewModel>();
            foreach (var assignment in assignments)
            {
                var dashboardItem = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment);
                dashboardItems.Add(dashboardItem);
            }

            return dashboardItems;
        }
    }
}