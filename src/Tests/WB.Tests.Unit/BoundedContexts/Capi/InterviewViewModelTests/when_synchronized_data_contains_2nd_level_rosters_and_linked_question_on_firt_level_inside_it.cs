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
    internal class when_synchronized_data_contains_2nd_level_rosters_and_linked_question_on_firt_level_inside_it : InterviewViewModelTestContext
    {
        private Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
            firstLevelRosterId = Guid.Parse("10000000000000000000000000000000");
            secondLevelRosterId = Guid.Parse("44444444444444444444444444444444");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = firstLevelRosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        new Group()
                        {
                            PublicKey = secondLevelRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            RosterFixedTitles = new[] { "t1", "t2" },
                            Children = new List<IComposite>()
                            {
                                new SingleQuestion()
                                {
                                    PublicKey = linkedQuestionId,
                                    LinkedToQuestionId = sourceForLinkedQuestionId
                                }

                            }
                        },
                        new NumericQuestion() { PublicKey = sourceForLinkedQuestionId, QuestionType = QuestionType.Numeric }
                    }
                });


            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 0 }, 1, string.Empty),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 1 }, 2, string.Empty)
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(firstLevelRosterId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 0, null, null),
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 1, null, null),

                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 0, null, null)
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 0 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 0, null, null),
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 1, null, null),
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 1 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 0, null, null)
                        }
                    }
                });
        };

        Because of = () =>
          interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
              interviewSynchronizationDto);

        It should_linked_in_first_row_of_first_nested_roster_has_2_options = () =>
            ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0, 0 }))
                .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        It should_linked_in_second_row_of_first_nested_roster_has_2_options = () =>
         ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
             question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0, 1 }))
             .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        It should_linked_in_first_row_of_second_nested_roster_has_2_options = () =>
         ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
             question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 1, 0 }))
             .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        It should_linked_in_first_row_of_first_nested_roster_has_first_option_equal_to_11 = () =>
         ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
             question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0, 0 }))
             .FirstOrDefault()).AnswerOptions.First().Title.ShouldEqual("1");

        It should_linked_in_first_row_of_first_nested_roster_has_second_option_equal_to_12 = () =>
        ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
            question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0, 0 }))
            .FirstOrDefault()).AnswerOptions.Last().Title.ShouldEqual("2");

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid firstLevelRosterId;
        private static Guid linkedQuestionId;
        private static Guid secondLevelRosterId;
        private static Guid sourceForLinkedQuestionId;
    }
}
