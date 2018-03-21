using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_numeric_question_which_triggers_square_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterId = Guid.Parse("11111111111111111111111111111111");
            nestedRosterId = Guid.Parse("21111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: "num"),
                    Create.Entity.Roster(rosterId: rosterId, variable: "r1",
                        rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId,
                        children: new[]
                        {
                            Create.Entity.Roster(rosterId: nestedRosterId, variable: "r2",
                                rosterSizeSourceType: RosterSizeSourceType.Question,
                                rosterSizeQuestionId: rosterSizeQuestionId)
                        })
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                 Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);


            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 2);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesTitleChanged_event_for_nested_roster () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Count(x => x.RosterInstance.GroupId == nestedRosterId)==4);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesTitleChanged_event_for_roster () =>
           eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
               => @event.ChangedInstances.Count(x => x.RosterInstance.GroupId == rosterId) == 2);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterId;
        private static Guid nestedRosterId;
    }
}
