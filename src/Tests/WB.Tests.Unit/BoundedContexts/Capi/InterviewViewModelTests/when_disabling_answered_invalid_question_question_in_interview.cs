using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_disabling_answered_invalid_question_question_in_interview : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            targetQuestionId = Guid.Parse("33333333333333333333333333333333");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = targetQuestionId,
                    QuestionType = QuestionType.Numeric
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
            interviewSynchronizationDto);
            interviewViewModel.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(targetQuestionId, new decimal[0]), 3);
            interviewViewModel.SetQuestionValidity(ConversionHelper.ConvertIdAndRosterVectorToString(targetQuestionId, new decimal[0]), false);
        };

        Because of = () =>
             interviewViewModel.SetQuestionStatus(ConversionHelper.ConvertIdAndRosterVectorToString(targetQuestionId, new decimal[0]), false);

        It should_UnansweredQuestions_in_statistic_cont_has_zero_elements = () =>
           interviewViewModel.Statistics.UnansweredQuestions.ShouldBeEmpty();

        It should_answeredQuestions_in_statistic_cont_has_zero_elements = () =>
           interviewViewModel.Statistics.AnsweredQuestions.ShouldBeEmpty();

        It should_invalidQuestions_in_statistic_be_empty = () =>
          interviewViewModel.Statistics.InvalidQuestions.ShouldBeEmpty();

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid targetQuestionId;
    }
}
