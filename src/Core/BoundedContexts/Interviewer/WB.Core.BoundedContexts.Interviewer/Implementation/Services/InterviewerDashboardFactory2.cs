using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;


namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    public class InterviewerDashboardFactory2 : IInterviewerDashboardFactory
    {
        private readonly IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage;
        private readonly IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage;
        private readonly IAsyncPlainStorage<QuestionnireInfo> plainStorageQuestionnireCensusInfo;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;

        public InterviewerDashboardFactory2(IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage,
            IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage,
            IAsyncPlainStorage<QuestionnireInfo> plainStorageQuestionnireCensusInfo,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository)
        {
            this.questionnaireDtoDocumentStorage = questionnaireDtoDocumentStorage;
            this.surveyDtoDocumentStorage = surveyDtoDocumentStorage;
            this.plainStorageQuestionnireCensusInfo = plainStorageQuestionnireCensusInfo;
            this.questionnaireRepository = questionnaireRepository;
        }

//        public DashboardInformation Load(DashboardInput input)
//        {
//            var result = new DashboardInformation();
//
//            var userId = input.UserId.FormatGuid();
//
//            var surveys = this.surveyDtoDocumentStorage.Filter(s => true).ToList();
//            List<QuestionnaireDTO> questionnaires = this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();
//
//            foreach (SurveyDto surveyDto in surveys)
//            {
//                var interviews = new List<QuestionnaireDTO>();
//                if (string.IsNullOrEmpty(surveyDto.QuestionnaireId))
//                {
//                    interviews.AddRange(questionnaires.Where(q => q.Survey == surveyDto.Id));
//                }
//                else
//                {
//                    interviews.AddRange(
//                        questionnaires.Where(q => q.Survey == surveyDto.QuestionnaireId && q.SurveyVersion == surveyDto.QuestionnaireVersion));
//                }
//
//                if (interviews.Any() || surveyDto.AllowCensusMode)
//                {
//                    result.Surveys.Add(new DashboardSurveyItem(surveyDto.Id,
//                        surveyDto.QuestionnaireId,
//                        surveyDto.QuestionnaireVersion,
//                        surveyDto.SurveyTitle,
//                        interviews.Select(
//                            i =>
//                                new DashboardQuestionnaireItem(Guid.Parse(i.Id), Guid.Parse(i.Survey), (InterviewStatus)i.Status,
//                                    i.GetProperties(), surveyDto.SurveyTitle, i.Comments, i.CreatedOnClient,
//                                    i.JustInitilized.HasValue && i.JustInitilized.Value)),
//                        surveyDto.AllowCensusMode));
//                }
//            }
//            return result;
//        }





//        private readonly IStatefulInterviewRepository aggregateRootRepository;
//        private readonly IAsyncPlainStorage<QuestionnireInfo> plainStorageQuestionnireCensusInfo;
//
//        public InterviewerDashboardFactory2(IStatefulInterviewRepository aggregateRootRepository,
//            IAsyncPlainStorage<QuestionnireInfo> plainStorageQuestionnireCensusInfo)
//        {
//            this.aggregateRootRepository = aggregateRootRepository;
//            this.plainStorageQuestionnireCensusInfo = plainStorageQuestionnireCensusInfo;
//        }

        public Task<DashboardInformation> GetDashboardItems(Guid interviewerId, DashboardInterviewCategories category)
        {
            return Task.Run(() => this.CollectDashboardInformation(new DashboardInput(interviewerId)));
        }

        private DashboardInformation CollectDashboardInformation(DashboardInput input)
        {
            var dashboardInformation = new DashboardInformation();

            var userId = input.UserId.FormatGuid();

            var surveys = this.surveyDtoDocumentStorage.Filter(s => true).ToList();
            //plainStorageQuestionnireCensusInfo.Query(q => q.ToList());
            List<QuestionnaireDTO> questionnaires = this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();

            var listCensusQuestionnires = surveys.Where(s => s.AllowCensusMode);
            // show census mode for new tab
            //var listCensusQuestionnires = this.plainStorageQuestionnireCensusInfo.Query(_ => _.Where(questionnaire => questionnaire.AllowCensus).ToList());
            foreach (var censusQuestionnireInfo in listCensusQuestionnires)
            {
                var censusQuestionnaireDashboardItem = Load<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireInfo);
                dashboardInformation.CensusQuestionniories.Add(censusQuestionnaireDashboardItem);
            }

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

                dashboardQuestionnaireItems.Add(
                    new DashboardQuestionnaireItem(Guid.Parse(questionnaire.Id),
                        Guid.Parse(questionnaire.Survey), 
                        (InterviewStatus)questionnaire.Status,
                        questionnaire.GetProperties(), 
                        survey.SurveyTitle, 
                        questionnaire.Comments, 
                        questionnaire.CreatedOnClient,
                        questionnaire.JustInitilized.HasValue && questionnaire.JustInitilized.Value));
            }


            foreach (var dashboardQuestionnaireItem in dashboardQuestionnaireItems)
            {
                var interviewCategory = this.GetDashboardCategoryForInterview(dashboardQuestionnaireItem);
                var interviewDashboardItem = Load<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(dashboardQuestionnaireItem, interviewCategory);
                this.AddDashboardItemToCategoryCollection(dashboardInformation, interviewCategory, interviewDashboardItem);
            }

            return dashboardInformation;




//            foreach (SurveyDto surveyDto in surveys)
//            {
//                var interviews = new List<QuestionnaireDTO>();
//                if (string.IsNullOrEmpty(surveyDto.QuestionnaireId))
//                {
//                    interviews.AddRange(questionnaires.Where(q => q.Survey == surveyDto.Id));
//                }
//                else
//                {
//                    interviews.AddRange(questionnaires.Where(q => q.Survey == surveyDto.QuestionnaireId && q.SurveyVersion == surveyDto.QuestionnaireVersion));
//                }
//
//
//                if (interviews.Any() || surveyDto.AllowCensusMode)
//                {
//                    // show census mode for new tab
//                    var listCensusQuestionnires = this.plainStorageQuestionnireCensusInfo.Query(_ => _.Where(questionnaire=>questionnaire.AllowCensus).ToList());
//                    foreach (var censusQuestionnireInfo in listCensusQuestionnires)
//                    {
//                        var censusQuestionnaireDashboardItem = Load<CensusQuestionnaireDashboardItemViewModel>();
//                        censusQuestionnaireDashboardItem.Init(censusQuestionnireInfo.Id);
//                        dashboardInformation.NewInterviews.Add(censusQuestionnaireDashboardItem);
//                    }
//
//
//                    // collect all interviews statistics ans show interview for current tab
//                    var interviewAggregateRoots = this.aggregateRootRepository.GetAll();
//
//                    foreach (var interview in interviewAggregateRoots)
//                    {
//                        var interviewCategory = this.GetDashboardCategoryForInterview(interview);
//                        var interviewDashboardItem = Load<InterviewDashboardItemViewModel>();
//                        interviewDashboardItem.Init(interview, interviewCategory);
//                        this.AddDashboardItemToCategoryCollection(dashboardInformation, interviewCategory, interviewDashboardItem);
//                    }
//
//                    dashboardInformation.Surveys.Add(new DashboardSurveyItem(surveyDto.Id,
//                        surveyDto.QuestionnaireId,
//                        surveyDto.QuestionnaireVersion,
//                        surveyDto.SurveyTitle,
//                        interviews.Select(
//                            i =>
//                                new DashboardQuestionnaireItem(Guid.Parse(i.Id), Guid.Parse(i.Survey), (InterviewStatus)i.Status,
//                                    i.GetProperties(), surveyDto.SurveyTitle, i.Comments, i.CreatedOnClient,
//                                    i.JustInitilized.HasValue && i.JustInitilized.Value)),
//                        surveyDto.AllowCensusMode));
//                }
//            }
        }

        //private List<QuestionnaireDTO> GetInterviewsFor

        private DashboardInterviewCategories GetDashboardCategoryForInterview(DashboardQuestionnaireItem interview)
        {
            switch (interview.Status)
            {
                case InterviewStatus.RejectedBySupervisor:
                    return DashboardInterviewCategories.Rejected;
                case InterviewStatus.Completed:
                    return DashboardInterviewCategories.Complited;
                case InterviewStatus.Restarted:
                    return DashboardInterviewCategories.InProgress;
                case InterviewStatus.InterviewerAssigned:
                {
//                    if (interview.Answers.Count > 0)
//                        return DashboardInterviewCategories.InProgress;
//                    else
                        return DashboardInterviewCategories.New;
                }

                default:
                    throw new ArgumentException("Can't identify status for interview: {0}".FormatString(interview.SurveyKey));
            }
        }

        private void AddDashboardItemToCategoryCollection(DashboardInformation dashboardInformation, 
            DashboardInterviewCategories category, InterviewDashboardItemViewModel interviewDashboardItem)
        {
            switch (category)
            {
                case DashboardInterviewCategories.Rejected:
                    dashboardInformation.RejectedInterviews.Add(interviewDashboardItem);
                    break;
                case DashboardInterviewCategories.Complited:
                    dashboardInformation.CompletedInterviews.Add(interviewDashboardItem);
                    break;
                case DashboardInterviewCategories.New:
                    dashboardInformation.NewInterviews.Add(interviewDashboardItem);
                    break;
                case DashboardInterviewCategories.InProgress:
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