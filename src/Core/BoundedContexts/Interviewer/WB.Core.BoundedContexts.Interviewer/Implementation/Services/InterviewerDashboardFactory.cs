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

        public async Task<DashboardInformation> GetInterviewerDashboardAsync(Guid interviewerId)
        {
            var censusQuestionnaires = await this.GetCensusQuestionnairesAsync();
            var interviews = await this.GetInterivewsByInterviewerIdAsync(interviewerId);

            return new DashboardInformation(
                censusQuestionnaires: censusQuestionnaires.ToList<IDashboardItem>(),
                newInterviews: interviews.Where(i => i.Status == DashboardInterviewStatus.New).ToList<IDashboardItem>(),
                startedInterviews: interviews.Where(i => i.Status == DashboardInterviewStatus.InProgress).ToList<IDashboardItem>(),
                completedInterviews: interviews.Where(i => i.Status == DashboardInterviewStatus.Completed).ToList<IDashboardItem>(),
                rejectedInterviews: interviews.Where(i => i.Status == DashboardInterviewStatus.Rejected).ToList<IDashboardItem>());
        }

        private async Task<List<CensusQuestionnaireDashboardItemViewModel>> GetCensusQuestionnairesAsync()
        {
            var censusQuestionnireViews =
                await this.questionnaireViewRepository.WhereAsync(questionnaire => questionnaire.Census);

            // show census mode for new tab
            var censusQuestionnaireViewModels = new List<CensusQuestionnaireDashboardItemViewModel>();
            foreach (var censusQuestionnireView in censusQuestionnireViews)
            {
                var censusQuestionnaireDashboardItem = this.interviewViewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                await censusQuestionnaireDashboardItem.Init(censusQuestionnireView);
                censusQuestionnaireViewModels.Add(censusQuestionnaireDashboardItem);
            }

            return censusQuestionnaireViewModels;
        }

        private async Task<List<InterviewDashboardItemViewModel>> GetInterivewsByInterviewerIdAsync(Guid interviewerId)
        {
            var interviewViews = await this.interviewViewRepository.WhereAsync(interview => interview.ResponsibleId == interviewerId);

            var interviewViewModels = new List<InterviewDashboardItemViewModel>();
            foreach (var interviewView in interviewViews)
            {
                var interviewDashboardItem = this.interviewViewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);
                interviewViewModels.Add(interviewDashboardItem);
            }

            return interviewViewModels;
        }
    }
}