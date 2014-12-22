using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class when_synchronized_data_contains_roster_with_2_rows_and_linked_question_out_of_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
            propagatedGroupId = Guid.Parse("10000000000000000000000000000000");
            linkedQuestionInRosterId = Guid.Parse("44444444444444444444444444444444");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = propagatedGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        new NumericQuestion() { PublicKey = sourceForLinkedQuestionId },
                        new SingleQuestion() { PublicKey = linkedQuestionInRosterId, LinkedToQuestionId = sourceForLinkedQuestionId }
                    }
                },
                new SingleQuestion()
                {
                    PublicKey = linkedQuestionId,
                    LinkedToQuestionId = sourceForLinkedQuestionId
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
                        new InterviewItemId(propagatedGroupId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(propagatedGroupId, new decimal[0], 0, null, null),
                            new RosterSynchronizationDto(propagatedGroupId, new decimal[0], 1, null, null)
                        }
                    }
                });
        };

        Because of = () =>
          interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
              interviewSynchronizationDto);

        It should_linked_question_outside_roster_has_2_options = () =>
            ((LinkedQuestionViewModel) interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[0]))
                .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        It should_first_linked_question_inside_roster_has_2_options = () =>
           ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
               question => question.PublicKey == new InterviewItemId(linkedQuestionInRosterId, new decimal[] { 0 }))
               .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        It should_second_linked_question_inside_roster_has_2_options = () =>
          ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
              question => question.PublicKey == new InterviewItemId(linkedQuestionInRosterId, new decimal[] { 1 }))
              .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(2);

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid propagatedGroupId;
        private static Guid linkedQuestionId;
        private static Guid linkedQuestionInRosterId;
        private static Guid sourceForLinkedQuestionId;
    }
}
