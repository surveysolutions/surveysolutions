using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerDashboardFactory : IInterviewerDashboardFactory
    {
        private readonly IStatefulInterviewRepository aggregateRootRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IAnswerToStringService answerToStringService;

        public InterviewerDashboardFactory(IStatefulInterviewRepository aggregateRootRepository, 
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IAnswerToStringService answerToStringService)
        {
            this.aggregateRootRepository = aggregateRootRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.answerToStringService = answerToStringService;
        }

        public IEnumerable<DashboardItemViewModel> GetDashboardItems(Guid interviewerId)
        {
            var interviewAggregateRoots = aggregateRootRepository.GetAll();

            foreach (var interview in interviewAggregateRoots)
            {
//                if (interview.InterviewerId != interviewerId)
//                    continue;

                var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

                DashboardItemViewModel dashboardItem = new DashboardItemViewModel();
                dashboardItem.InterviewId = interview.Id;
                dashboardItem.QuestionariName = questionnaire.Title;
                dashboardItem.ComplitedDate = DateTime.Now;
                dashboardItem.RejectedDate = DateTime.Now;
                dashboardItem.StartedDate = DateTime.Now;
                dashboardItem.Status = interview.Status;

                dashboardItem.PrefilledQuestions = new List<PrefilledQuestion>();
                var prefilledQuestionsIds = questionnaire.PrefilledQuestionsIds;
                foreach (var prefilledQuestionsId in prefilledQuestionsIds)
                {
                    var baseQuestionModel = questionnaire.Questions[prefilledQuestionsId.Id];
                    var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(prefilledQuestionsId.Id, new decimal[0]);
                    var baseInterviewAnswer = interview.Answers[identityAsString];

                    var prefilledQuestion = new PrefilledQuestion();
                    prefilledQuestion.Question = baseQuestionModel.Title;
                    prefilledQuestion.Answer = answerToStringService.AnswerToUIString(baseQuestionModel,
                        baseInterviewAnswer, interview, questionnaire);
                    dashboardItem.PrefilledQuestions.Add(prefilledQuestion);
                }

                yield return dashboardItem;
            }
        }
    }
}