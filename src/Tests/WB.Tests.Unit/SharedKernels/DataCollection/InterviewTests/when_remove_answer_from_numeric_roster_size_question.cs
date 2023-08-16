using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_remove_answer_from_numeric_roster_size_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId, variable: "num"),
                    Create.Entity.Roster(rosterId: rosterId, variable: "ros",
                        rosterSizeQuestionId: questionWhichIncreasesRosterSizeId,
                        rosterSizeSourceType: RosterSizeSourceType.Question)
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0],
                DateTime.Now, 2);
            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.RemoveAnswer(questionWhichIncreasesRosterSizeId, new decimal[0], userId, DateTime.Now);

        [NUnit.Framework.Test] public void should_raise_AnswerRemoved_event_for_first_row () =>
            eventContext.GetEvent<AnswersRemoved>().Questions.Should()
                .BeEquivalentTo(new[]{
                    Create.Entity.Identity(questionWhichIncreasesRosterSizeId, RosterVector.Empty)});

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesAdded_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>();

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_for_first_roster_row () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterId && instance.RosterInstanceId==0));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_for_second_roster_row () =>
          eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
              => @event.Instances.Any(instance => instance.GroupId == rosterId && instance.RosterInstanceId == 1));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid rosterId;
    }
}
