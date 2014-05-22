using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_adding_roster_row_and_row_contains_question_with_substitution_on_upper_level_of_interview : InterviewViewModelTestContext
    {
        private Establish context = () =>
        {
            questionWithSubstitutionInTitleId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("10000000000000000000000000000000");

            substitutionSourceQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = substitutionSourceQuestionId,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = "var"
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterFixedTitles = new []{"1","2"},
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>()
                    {
                        new NumericQuestion() { PublicKey = questionWithSubstitutionInTitleId, QuestionType = QuestionType.Numeric, QuestionText = "I'am substitute %var%"}
                    }
                });


            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(substitutionSourceQuestionId, new decimal[0], 1, string.Empty)
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
            interviewSynchronizationDto);
        };

        Because of = () =>
            PropagateScreen(interviewViewModel, rosterId, 0, new decimal[0]);

        It should_question_title_in_added_roster_row_be_substituted_with_answer_on_substitution_reference_question = () =>
            interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(questionWithSubstitutionInTitleId, new decimal[] { 0 }))
                .FirstOrDefault().Text.ShouldEqual("I'am substitute 1");

        It should_unansweredQuestions_in_statistic_contain_questionWithSubstitutionInTitleId = () =>
         interviewViewModel.Statistics.UnansweredQuestions.Any(q => q.PublicKey == new InterviewItemId(questionWithSubstitutionInTitleId, new decimal[]{0})).ShouldBeTrue();

        It should_count_of_unansweredQuestions_in_statistic_be_equal_to_1 = () =>
           interviewViewModel.Statistics.UnansweredQuestions.Count.ShouldEqual(1);

        It should_answeredQuestions_in_statistic_contain_substitutionSourceQuestionId = () =>
          interviewViewModel.Statistics.AnsweredQuestions.Any(q => q.PublicKey == new InterviewItemId(substitutionSourceQuestionId, new decimal[0])).ShouldBeTrue();

        It should_count_of_answeredQuestions_in_statistic_be_equal_to_1 = () =>
           interviewViewModel.Statistics.AnsweredQuestions.Count.ShouldEqual(1);

        It should_invalidQuestions_in_statistic_be_empty = () =>
          interviewViewModel.Statistics.InvalidQuestions.ShouldBeEmpty();

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid questionWithSubstitutionInTitleId;
        private static Guid substitutionSourceQuestionId;
    }
}
