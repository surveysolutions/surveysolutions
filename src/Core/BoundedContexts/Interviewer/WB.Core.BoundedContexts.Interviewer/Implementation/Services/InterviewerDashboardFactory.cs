using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;


namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerDashboardFactory : IInterviewerDashboardFactory
    {
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;

        public InterviewerDashboardFactory(IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory interviewViewModelFactory)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
        }

        public Task<DashboardInformation> GetInterviewerDashboardAsync(Guid interviewerId)
        {
            return Task.Run(() => this.GetInterviewerDashboard(interviewerId));
        }

        private DashboardInformation GetInterviewerDashboard(Guid interviewerId)
        {
            var censusQuestionnaires = this.GetCensusQuestionnaires();
            var interviews = this.GetInterivewsByInterviewerId(interviewerId);

            return new DashboardInformation(censusQuestionnaires: censusQuestionnaires,
                interviews: interviews);
        }

        private IEnumerable<CensusQuestionnaireDashboardItemViewModel> GetCensusQuestionnaires()
        {
            var censusQuestionnireViews =
                this.questionnaireViewRepository.Query(questionnaires => questionnaires.Where(questionnaire => questionnaire.Census).ToList());

            // show census mode for new tab
            foreach (var censusQuestionnireView in censusQuestionnireViews)
            {
                var censusQuestionnaireDashboardItem = this.interviewViewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireView.Id);
                yield return censusQuestionnaireDashboardItem;
            }
        }

        private IEnumerable<InterviewDashboardItemViewModel> GetInterivewsByInterviewerId(Guid interviewerId)
        {
            var interviewViews = this.interviewViewRepository.Query(interviews =>
                interviews.Where(interview => interview.ResponsibleId == interviewerId).ToList());

            foreach (var interviewView in interviewViews)
            {
                var interviewDashboardItem = this.interviewViewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView.Id);
                yield return interviewDashboardItem;
            }
        }
    }
}