using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class QuestionnairesAndNewInterviewsViewModel : ListViewModel<IDashboardItem>
    {
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;
        public int NewInterviewsCount => this.Items.OfType<InterviewDashboardItemViewModel>().Count();
        public int CensusQuestionnairesCount => this.Items.Count() - this.NewInterviewsCount;

        public QuestionnairesAndNewInterviewsViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.principal = principal;
        }

        public void Load()
        {
            var censusQuestionnaires = this.GetCensusQuestionnaires().ToList<IDashboardItem>();
            var newInterviews = this.GetNewInterviews().ToList();

            this.Items = censusQuestionnaires.Union(newInterviews).ToList();

            this.Title = string.Format(InterviewerUIResources.Dashboard_NewItemsLinkText, newInterviews.Count);
        }

        private List<CensusQuestionnaireDashboardItemViewModel> GetCensusQuestionnaires()
        {
            var censusQuestionnireViews = this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census);

            // show census mode for new tab
            var censusQuestionnaireViewModels = new List<CensusQuestionnaireDashboardItemViewModel>();
            foreach (var censusQuestionnireView in censusQuestionnireViews)
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireView);
                censusQuestionnaireViewModels.Add(censusQuestionnaireDashboardItem);
            }

            return censusQuestionnaireViewModels;
        }

        private IEnumerable<InterviewDashboardItemViewModel> GetNewInterviews()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;
            var interviewViews = this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == InterviewStatus.InterviewerAssigned &&
                interview.StartedDateTime == null);
            
            foreach (var interviewView in interviewViews)
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);

                yield return interviewDashboardItem;
            }
        }
    }
}