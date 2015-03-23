using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    internal class when_setting_validity_to_answered_question_in_interview : InterviewViewModelTestContext
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
        };

        Because of = () =>
            interviewViewModel.SetQuestionValidity(ConversionHelper.ConvertIdAndRosterVectorToString(targetQuestionId, new decimal[0]), false);

        It should_unansweredQuestions_in_statistic_count_has_zero_elements = () =>
           interviewViewModel.Statistics.UnansweredQuestions.ShouldBeEmpty();

        It should_answeredQuestions_in_statistic_contain_targetQuestionId = () =>
          interviewViewModel.Statistics.AnsweredQuestions.Any(q => q.PublicKey == new InterviewItemId(targetQuestionId, new decimal[0])).ShouldBeTrue();

        It should_count_of_answeredQuestions_in_statistic_be_equal_to_1 = () =>
           interviewViewModel.Statistics.AnsweredQuestions.Count.ShouldEqual(1);

        It should_invalidQuestions_in_statistic_contain_targetQuestionId = () =>
          interviewViewModel.Statistics.InvalidQuestions.Any(q => q.PublicKey == new InterviewItemId(targetQuestionId, new decimal[0])).ShouldBeTrue();

        It should_count_of_invalidQuestions_in_statistic_be_equal_to_1 = () =>
           interviewViewModel.Statistics.InvalidQuestions.Count.ShouldEqual(1);

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid targetQuestionId;
    }
}
