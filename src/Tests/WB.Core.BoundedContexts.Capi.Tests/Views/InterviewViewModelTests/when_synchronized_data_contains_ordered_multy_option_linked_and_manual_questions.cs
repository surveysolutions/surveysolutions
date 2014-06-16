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
    internal class when_synchronized_data_contains_ordered_multy_option_linked_and_manual_questions : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
            propagatedGroupId = Guid.Parse("10000000000000000000000000000000");
            manualMultyOptionQuestionId = Guid.Parse("44444444444444444444444444444444");


            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = propagatedGroupId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2","3" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>()
                    {
                        new NumericQuestion() { PublicKey = sourceForLinkedQuestionId, QuestionType = QuestionType.Numeric }
                    }
                },
                new MultyOptionsQuestion()
                {
                    PublicKey = linkedQuestionId,
                    LinkedToQuestionId = sourceForLinkedQuestionId,
                    QuestionType = QuestionType.MultyOption,
                    AreAnswersOrdered = true
                },
                new MultyOptionsQuestion()
                {
                    PublicKey = manualMultyOptionQuestionId,
                    Answers =
                        new List<Answer>
                        {
                            new Answer() { AnswerValue = "1", AnswerText = "1" },
                            new Answer() { AnswerValue = "2", AnswerText = "2" },
                            new Answer() { AnswerValue = "3", AnswerText = "3" }
                        },
                    QuestionType = QuestionType.MultyOption,
                    AreAnswersOrdered = true
                }
                );

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(linkedQuestionId, new decimal[0],
                        new[] { new decimal[] { 2 }, new decimal[] { 1 } }, null),
                    new AnsweredQuestionSynchronizationDto(manualMultyOptionQuestionId, new decimal[0], new decimal[] { 2, 1 }, null),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 0 }, 1, null),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 1 }, 2, null),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 2 }, 3, null)
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(propagatedGroupId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(propagatedGroupId, new decimal[0], 0, null, "1"),
                            new RosterSynchronizationDto(propagatedGroupId, new decimal[0], 1, null, "2"),
                            new RosterSynchronizationDto(propagatedGroupId, new decimal[0], 2, null, "3")
                        }
                    }
                });
        };

        Because of = () =>
          interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
              interviewSynchronizationDto);

        It should_answer_string_of_linked_multy_option_question_be_equal_to_3_2 = () =>
            ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[0]))
                .FirstOrDefault()).AnswerString.ShouldEqual("3, 2");

        It should_answer_string_of_manual_multy_option_question_be_equal_to_2_1 = () =>
            ((SelectebleQuestionViewModel)interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(manualMultyOptionQuestionId, new decimal[0]))
                .FirstOrDefault()).AnswerString.ShouldEqual("2, 1");

       /* It should_first_linked_question_inside_roster_has_2_options = () =>
           ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
               question => question.PublicKey == new InterviewItemId(manualMultyOptionQuestionId, new decimal[] { 0 }))
               .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        It should_second_linked_question_inside_roster_has_2_options = () =>
          ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
              question => question.PublicKey == new InterviewItemId(manualMultyOptionQuestionId, new decimal[] { 1 }))
              .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);*/

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid propagatedGroupId;
        private static Guid linkedQuestionId;
        private static Guid manualMultyOptionQuestionId;
        private static Guid sourceForLinkedQuestionId;
    }
}
