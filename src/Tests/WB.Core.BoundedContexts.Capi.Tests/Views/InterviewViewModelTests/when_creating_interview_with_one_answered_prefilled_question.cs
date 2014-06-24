using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_creating_interview_with_one_answered_prefilled_question : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = answeredQuestionId,
                    QuestionType = QuestionType.Numeric,
                    Featured = true
                });

            questionnaire.Title = testTemplate;

            rosterStructure =  new QuestionnaireRosterStructure
            {
                QuestionnaireId = questionnaire.PublicKey, 
                Version = 1
            };

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(answeredQuestionId, new decimal[0], answerForNumeric, string.Empty), 
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
        };

        Because of = () =>
            interviewViewModel = CreateInterviewViewModel(questionnaire, rosterStructure,
                interviewSynchronizationDto);

        It should_answeredQuestions_in_statistic_cont_has_zero_elements = () =>
            interviewViewModel.Title.ShouldEqual(string.Format("{0} | {1} ", testTemplate, answerForNumeric));

        It should_invalidQuestions_in_statistic_be_empty = () =>
            interviewViewModel.Statistics.InvalidQuestions.ShouldBeEmpty();

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid answeredQuestionId = Guid.Parse("33333333333333333333333333333333");
        private const string testTemplate = "test template";
        private const int answerForNumeric = 15;
    }
}