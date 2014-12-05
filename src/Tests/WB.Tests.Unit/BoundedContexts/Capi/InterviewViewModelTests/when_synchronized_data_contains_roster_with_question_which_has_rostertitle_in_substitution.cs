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

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_synchronized_data_contains_roster_with_question_which_has_rostertitle_in_substitution : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");
            questionWithSubstitutionId = Guid.Parse("11111111111111111111111111111111");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterFixedTitles = new []{"a","b"},
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionWithSubstitutionId, QuestionType = QuestionType.Numeric, QuestionText = "%rostertitle%"}
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(rosterId, new decimal[0]), new[]
                        {
                            new RosterSynchronizationDto(rosterId, new decimal[0], 0, null, "a"),
                            new RosterSynchronizationDto(rosterId, new decimal[0], 1, null, "b")
                        }
                    }
                });
        };

        Because of = () =>
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
                interviewSynchronizationDto);

        It should_answer_on_first_question_in_first_row_of_roster_equals_to_1 = () =>
            interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(questionWithSubstitutionId, new decimal[] { 0 }))
                .FirstOrDefault()
                .Text.ShouldEqual("a");

        It should_answer_on_first_question_in_second_row_of_roster_equals_to_2 = () =>
            interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(questionWithSubstitutionId, new decimal[] { 1 }))
                .FirstOrDefault()
                .Text.ShouldEqual("b");

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid rosterId;
        private static Guid questionWithSubstitutionId;
    }
}
