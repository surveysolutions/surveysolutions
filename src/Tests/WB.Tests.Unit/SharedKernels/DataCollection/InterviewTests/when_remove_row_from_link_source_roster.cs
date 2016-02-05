using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_remove_row_from_link_source_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
            linkedQuestionId = Guid.Parse("33222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocument(id: questionnaireId,
                children: new IComposite[]
                {

                    Create.NumericIntegerQuestion(id: rosterSizeQuestionId),
                    Create.Roster(rosterId: rosterId, variable: "ros", rosterSizeSourceType:RosterSizeSourceType.Question, rosterSizeQuestionId:rosterSizeQuestionId),
                    Create.SingleQuestion(id: linkedQuestionId, linkedToRosterId: rosterId)
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0],
                DateTime.Now, 2);
            interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, new decimal[0], DateTime.Now,
                new decimal[] { 0 });
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0],
                DateTime.Now, 0);

        It should_raise_RosterInstancesRemoved_event_for_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances[0].GroupId == rosterId && @event.Instances[0].RosterInstanceId==0);

        It should_raise_RosterInstancesRemoved_event_for_second_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances[1].GroupId == rosterId && @event.Instances[1].RosterInstanceId == 1);

        It should_raise_AnswersRemoved_event_for_answered_linked_Question = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event
                => @event.Questions.Count(q => q.Id == linkedQuestionId && !q.RosterVector.Any()) == 1);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static Guid linkedQuestionId;
        private static Guid rosterId;
    }
}