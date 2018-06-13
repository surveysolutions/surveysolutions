using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_remove_row_from_link_source_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
            linkedQuestionId = Guid.Parse("33222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.TextListQuestion(questionId: rosterSizeQuestionId, variable: "txt"),
                Abc.Create.Entity.Roster(rosterId: rosterId, variable: "ros", rosterSizeSourceType:RosterSizeSourceType.Question, rosterSizeQuestionId:rosterSizeQuestionId),
                Abc.Create.Entity.SingleQuestion(id: linkedQuestionId, linkedToRosterId: rosterId, variable: "link")
            });

            interview = SetupInterview(questionnaire);
            interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                new Tuple<decimal, string>(0, "a"),
                new Tuple<decimal, string>(1, "b")
            });
            interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.Now, new decimal[] { 0 });
            eventContext = new EventContext();

            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, new decimal[0],
                DateTime.Now, new[] { new Tuple<decimal, string>(1, "b") });

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_for_first_row () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances[0].GroupId == rosterId && @event.Instances[0].RosterInstanceId==0);

        [NUnit.Framework.Test] public void should_raise_AnswersRemoved_event_for_answered_linked_Question () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event
                => @event.Questions.Count(q => q.Id == linkedQuestionId && !q.RosterVector.Any()) == 1);

        [NUnit.Framework.Test] public void should_raise_LinkedOptionsChanged_event_for_answered_linked_Question () =>
          eventContext.ShouldContainEvent<LinkedOptionsChanged>(@event
              => @event.ChangedLinkedQuestions.Count(q => q.QuestionId == Create.Identity(linkedQuestionId) && q.Options.Length==1) == 1);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static Guid linkedQuestionId;
        private static Guid rosterId;
    }
}
