using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
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

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Create.ListQuestion(id: rosterSizeQuestionId, variable: "txt"),
                Create.Roster(id: rosterId, variable: "ros", rosterSizeSourceType:RosterSizeSourceType.Question, rosterSizeQuestionId:rosterSizeQuestionId),
                Create.SingleQuestion(id: linkedQuestionId, linkedToRosterId: rosterId, variable: "link")
            });

            interview = SetupInterview(questionnaire);
            interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                new Tuple<decimal, string>(0, "a"),
                new Tuple<decimal, string>(1, "b")
            });
            interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.Now, new decimal[] { 0 });
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, new decimal[0],
                DateTime.Now, new[] { new Tuple<decimal, string>(1, "b") });

        It should_raise_RosterInstancesRemoved_event_for_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances[0].GroupId == rosterId && @event.Instances[0].RosterInstanceId==0);

        It should_raise_AnswersRemoved_event_for_answered_linked_Question = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event
                => @event.Questions.Count(q => q.Id == linkedQuestionId && !q.RosterVector.Any()) == 1);

        It should_raise_LinkedOptionsChanged_event_for_answered_linked_Question = () =>
          eventContext.ShouldContainEvent<LinkedOptionsChanged>(@event
              => @event.ChangedLinkedQuestions.Count(q => q.QuestionId == new Identity(linkedQuestionId,new decimal[0]) && q.Options.Length==1) == 1);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static Guid linkedQuestionId;
        private static Guid rosterId;
    }
}