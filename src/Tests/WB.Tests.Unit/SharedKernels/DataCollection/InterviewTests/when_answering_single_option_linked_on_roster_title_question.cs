using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_single_option_linked_on_roster_title_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    =>

                        _.HasQuestion(linkedToQuestionId) == true
                       && _.GetQuestionType(linkedToQuestionId) == QuestionType.SingleOption
                       && _.GetQuestionReferencedByLinkedQuestion(linkedToQuestionId) == linkedToQuestionId
                       && _.IsQuestionLinkedToRoster(linkedToQuestionId) == true
                       && _.GetRosterReferencedByLinkedQuestion(linkedToQuestionId) == rosterId
                );
            IPlainQuestionnaireRepository questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.RosterInstancesAdded(rosterId));

            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], selectedRosterVector: answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionLinkedQuestionAnswered_event = () =>
          eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static decimal[] answer = { 0 };
    }
}