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
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModel
{
    internal class whent_synchronized_data_contains_roster_with_2_rows_and_linked_question_out_of_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
            propagatedGroupId = Guid.Parse("10000000000000000000000000000000");
            linkedQuestionInRosterId = Guid.Parse("44444444444444444444444444444444");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            var rosterGroup = new Group() { PublicKey = propagatedGroupId, IsRoster = true, RosterSizeQuestionId =rosterSizeQuestionId };
            rosterGroup.Children.Add(new NumericQuestion() { PublicKey = sourceForLinkedQuestionId });
            rosterGroup.Children.Add(new SingleQuestion(){PublicKey = linkedQuestionInRosterId,LinkedToQuestionId = sourceForLinkedQuestionId});

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                rosterGroup,
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
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, List<decimal>>()
                {
                    { new InterviewItemId(propagatedGroupId, new decimal[0]), new List<decimal>{ 0, 1 } }
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

        private static WB.Core.BoundedContexts.Capi.Views.InterviewDetails.InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid propagatedGroupId;
        private static Guid linkedQuestionId;
        private static Guid linkedQuestionInRosterId;
        private static Guid sourceForLinkedQuestionId;
    }
}
