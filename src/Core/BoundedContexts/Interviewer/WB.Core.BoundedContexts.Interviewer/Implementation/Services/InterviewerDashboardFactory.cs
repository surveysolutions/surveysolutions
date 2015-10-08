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

            var questionnaires = this.surveyDtoDocumentStorage.Filter(s => true).ToList();
            List<QuestionnaireDTO> interviews = this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();

            var censusQuestionnaireDashboardItemViewModels = this.CollectCensusQuestionnaries(interviews, questionnaires);
            var interviewDashboardItemViewModels = this.CollectInterviews(interviews, questionnaires);
            var dashboardInformation = new DashboardInformation(censusQuestionnaireDashboardItemViewModels, interviewDashboardItemViewModels);
            return dashboardInformation;
        }

        private IEnumerable<CensusQuestionnaireDashboardItemViewModel> CollectCensusQuestionnaries(List<QuestionnaireDTO> interviews, List<SurveyDto> questionnaires)
        {
            var listCensusQuestionnires = questionnaires.Where(s => s.AllowCensusMode);
            // show census mode for new tab
            foreach (var censusQuestionnireInfo in listCensusQuestionnires)
            {
                var countInterviewsFromCurrentQuestionnare = interviews.Count(questionnaire => IsInterviewForQuestionnaire(censusQuestionnireInfo, questionnaire));
                var censusQuestionnaireDashboardItem = Load<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireInfo, countInterviewsFromCurrentQuestionnare);
                yield return censusQuestionnaireDashboardItem;
            }
        }

        private IEnumerable<InterviewDashboardItemViewModel> CollectInterviews(List<QuestionnaireDTO> interviews, List<SurveyDto> questionnaires)
        {
            foreach (var interview in interviews)
            {
                var questionnaire = questionnaires.Single(interviewDto => IsInterviewForQuestionnaire(interviewDto, interview));
                var interviewCategory = this.GetDashboardCategoryForInterview((InterviewStatus)interview.Status, interview.StartedDateTime);

                var dashboardQuestionnaireItem = new DashboardQuestionnaireItem(Guid.Parse(interview.Id),
                        Guid.Parse(interview.Survey),
                        interviewCategory,
                        interview.GetPrefilledQuestions(),
                        questionnaire.SurveyTitle,
                        questionnaire.QuestionnaireVersion,
                        interview.Comments,
                        interview.StartedDateTime,
                        interview.CompletedDateTime,
                        interview.CreatedDateTime,
                        interview.RejectedDateTime,
                        interview.GpsLocationLatitude.HasValue && interview.GpsLocationLongitude.HasValue
                            ? new GpsCoordinatesViewModel(interview.GpsLocationLatitude.Value, interview.GpsLocationLongitude.Value)
                            : null,
                        interview.CreatedOnClient,
                        interview.JustInitilized.HasValue && interview.JustInitilized.Value);
 
                var interviewDashboardItem = Load<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(dashboardQuestionnaireItem);
                yield return interviewDashboardItem;
            }
        }

        private static bool IsInterviewForQuestionnaire(SurveyDto questionnaire, QuestionnaireDTO interview)
        {
            if (string.IsNullOrEmpty(questionnaire.QuestionnaireId))
            {
                return interview.Survey == questionnaire.Id;
            }
            else
            {
                return interview.Survey == questionnaire.QuestionnaireId
                       && interview.SurveyVersion == questionnaire.QuestionnaireVersion;
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