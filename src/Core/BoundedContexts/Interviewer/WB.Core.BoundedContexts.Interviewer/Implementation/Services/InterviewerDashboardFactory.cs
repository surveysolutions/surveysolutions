using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
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

        public InterviewerDashboardFactory(IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage,
            IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage)
        {
            this.questionnaireDtoDocumentStorage = questionnaireDtoDocumentStorage;
            this.surveyDtoDocumentStorage = surveyDtoDocumentStorage;
        }

        public Task<DashboardInformation> GetDashboardItems(Guid interviewerId, DashboardInterviewStatus status)
        {
            return Task.Run(() => this.CollectDashboardInformation(new DashboardInput(interviewerId)));
        }

        private DashboardInformation CollectDashboardInformation(DashboardInput input)
        {
            var dashboardInformation = new DashboardInformation();

            var userId = input.UserId.FormatGuid();

            var surveys = this.surveyDtoDocumentStorage.Filter(s => true).ToList();
            List<QuestionnaireDTO> questionnaires = this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();

            CollectCensusQuestionnaries(surveys, dashboardInformation);
            this.CollectInterviews(questionnaires, surveys, dashboardInformation);

            return dashboardInformation;
        }

        private static void CollectCensusQuestionnaries(List<SurveyDto> surveys, DashboardInformation dashboardInformation)
        {
            var listCensusQuestionnires = surveys.Where(s => s.AllowCensusMode);
            // show census mode for new tab
            foreach (var censusQuestionnireInfo in listCensusQuestionnires)
            {
                var censusQuestionnaireDashboardItem = Load<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireInfo);
                dashboardInformation.CensusQuestionniories.Add(censusQuestionnaireDashboardItem);
            }
        }

        private void CollectInterviews(List<QuestionnaireDTO> questionnaires, List<SurveyDto> surveys, DashboardInformation dashboardInformation)
        {
            List<DashboardQuestionnaireItem> dashboardQuestionnaireItems = new List<DashboardQuestionnaireItem>();

            foreach (var questionnaire in questionnaires)
            {
                var survey = surveys.First(surveyDto =>
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
                });

                var interviewCategory = this.GetDashboardCategoryForInterview((InterviewStatus)questionnaire.Status, questionnaire.StartedDateTime);

                dashboardQuestionnaireItems.Add(
                    new DashboardQuestionnaireItem(Guid.Parse(questionnaire.Id),
                        Guid.Parse(questionnaire.Survey),
                        interviewCategory,
                        questionnaire.GetProperties(),
                        survey.SurveyTitle,
                        survey.QuestionnaireVersion,
                        questionnaire.Comments,
                        questionnaire.StartedDateTime,
                        questionnaire.ComplitedDateTime,
                        questionnaire.CreatedDateTime,
                        questionnaire.CreatedOnClient,
                        questionnaire.JustInitilized.HasValue && questionnaire.JustInitilized.Value));
            }


            foreach (var dashboardQuestionnaireItem in dashboardQuestionnaireItems)
            {
                var interviewDashboardItem = Load<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(dashboardQuestionnaireItem);
                this.AddDashboardItemToCategoryCollection(dashboardInformation, interviewDashboardItem);
            }
        }

        private DashboardInterviewStatus GetDashboardCategoryForInterview(InterviewStatus interviewStatus, DateTime? startedDateTime)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.RejectedBySupervisor:
                    return DashboardInterviewStatus.Rejected;
                case InterviewStatus.Completed:
                    return DashboardInterviewStatus.Complited;
                case InterviewStatus.Restarted:
                    return DashboardInterviewStatus.InProgress;
                case InterviewStatus.InterviewerAssigned:
                {
                    if (startedDateTime.HasValue)
                        return DashboardInterviewStatus.InProgress;
                    else
                        return DashboardInterviewStatus.New;
                }

                default:
                    throw new ArgumentException("Can't identify status for interview: {0}".FormatString(interviewStatus));
            }
        }

        private void AddDashboardItemToCategoryCollection(DashboardInformation dashboardInformation, 
            InterviewDashboardItemViewModel interviewDashboardItem)
        {
            switch (interviewDashboardItem.Status)
            {
                case DashboardInterviewStatus.Rejected:
                    dashboardInformation.RejectedInterviews.Add(interviewDashboardItem);
                    break;
                case DashboardInterviewStatus.Complited:
                    dashboardInformation.CompletedInterviews.Add(interviewDashboardItem);
                    break;
                case DashboardInterviewStatus.New:
                    dashboardInformation.NewInterviews.Add(interviewDashboardItem);
                    break;
                case DashboardInterviewStatus.InProgress:
                    dashboardInformation.StartedInterviews.Add(interviewDashboardItem);
                    break;
            }
        }

        private static T Load<T>() where T : class
        {
            return Mvx.Resolve<T>();
        }
    }
}