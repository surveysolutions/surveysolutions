using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Interviewer.ViewModel.Dashboard;


namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerDashboardFactory : IInterviewerDashboardFactory
    {
        private readonly IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage;
        private readonly IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage;
        private readonly IServiceLocator serviceLocator;

        public InterviewerDashboardFactory(IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage,
            IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage,
            IServiceLocator serviceLocator)
        {
            this.questionnaireDtoDocumentStorage = questionnaireDtoDocumentStorage;
            this.surveyDtoDocumentStorage = surveyDtoDocumentStorage;
            this.serviceLocator = serviceLocator;
        }

        public Task<DashboardInformation> GetDashboardItemsAsync(Guid interviewerId)
        {
            return Task.Run(() => this.CollectDashboardInformation(interviewerId));
        }

        private DashboardInformation CollectDashboardInformation(Guid interviewerId)
        {
            var userId = interviewerId.FormatGuid();

            var surveys = this.surveyDtoDocumentStorage.Filter(s => true).ToList();
            List<QuestionnaireDTO> questionnaires = this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();

            var censusQuestionnaireDashboardItemViewModels = this.CollectCensusQuestionnaries(questionnaires, surveys);
            var interviewDashboardItemViewModels = this.CollectInterviews(questionnaires, surveys);
            var dashboardInformation = new DashboardInformation(censusQuestionnaireDashboardItemViewModels, interviewDashboardItemViewModels);
            return dashboardInformation;
        }

        private IEnumerable<CensusQuestionnaireDashboardItemViewModel> CollectCensusQuestionnaries(List<QuestionnaireDTO> questionnaires, List<SurveyDto> surveys)
        {
            var listCensusQuestionnires = surveys.Where(s => s.AllowCensusMode);
            // show census mode for new tab
            foreach (var censusQuestionnireInfo in listCensusQuestionnires)
            {
                var countInterviewsFromCurrentQuestionnare = questionnaires.Count(questionnaire => IsSurveyForQuestionnaire(censusQuestionnireInfo, questionnaire));
                var censusQuestionnaireDashboardItem = Load<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireInfo, countInterviewsFromCurrentQuestionnare);
                yield return censusQuestionnaireDashboardItem;
            }
        }

        private IEnumerable<InterviewDashboardItemViewModel> CollectInterviews(List<QuestionnaireDTO> questionnaires, List<SurveyDto> surveys)
        {
            foreach (var survey in surveys)
            {
                var questionnaire = questionnaires.Single(q => IsSurveyForQuestionnaire(survey, q));
                var interviewCategory = this.GetDashboardCategoryForInterview((InterviewStatus)questionnaire.Status, questionnaire.StartedDateTime);

                var dashboardQuestionnaireItem = new DashboardQuestionnaireItem(Guid.Parse(questionnaire.Id),
                        Guid.Parse(questionnaire.Survey),
                        interviewCategory,
                        questionnaire.GetProperties(),
                        survey.SurveyTitle,
                        survey.QuestionnaireVersion,
                        questionnaire.Comments,
                        questionnaire.StartedDateTime,
                        questionnaire.CompletedDateTime,
                        questionnaire.CreatedDateTime,
                        questionnaire.CreatedOnClient,
                        questionnaire.JustInitilized.HasValue && questionnaire.JustInitilized.Value);
 
                var interviewDashboardItem = Load<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(dashboardQuestionnaireItem);
                yield return interviewDashboardItem;
            }
        }

        private static bool IsSurveyForQuestionnaire(SurveyDto surveyDto, QuestionnaireDTO questionnaire)
        {
            if (string.IsNullOrEmpty(surveyDto.QuestionnaireId))
            {
                return questionnaire.Survey == surveyDto.Id;
            }
            else
            {
                return questionnaire.Survey == surveyDto.QuestionnaireId
                       && questionnaire.SurveyVersion == surveyDto.QuestionnaireVersion;
            }
        }

        private DashboardInterviewStatus GetDashboardCategoryForInterview(InterviewStatus interviewStatus, DateTime? startedDateTime)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.RejectedBySupervisor:
                    return DashboardInterviewStatus.Rejected;
                case InterviewStatus.Completed:
                    return DashboardInterviewStatus.Completed;
                case InterviewStatus.Restarted:
                    return DashboardInterviewStatus.InProgress;
                case InterviewStatus.InterviewerAssigned:
                    return startedDateTime.HasValue 
                        ? DashboardInterviewStatus.InProgress 
                        : DashboardInterviewStatus.New;

                default:
                    throw new ArgumentException("Can't identify status for interview: {0}".FormatString(interviewStatus));
            }
        }

        private T Load<T>() where T : class
        {
            return serviceLocator.GetInstance<T>();
        }
    }
}