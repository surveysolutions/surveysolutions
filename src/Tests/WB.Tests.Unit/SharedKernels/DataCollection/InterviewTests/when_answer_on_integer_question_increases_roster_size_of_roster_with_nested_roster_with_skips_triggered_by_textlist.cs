using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_size_of_roster_with_nested_roster_with_skips_triggered_by_textlist : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            textListQuestionWhichIncreasesNestedRosterSizeId = Guid.Parse("31111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),
                Create.Entity.TextListQuestion(questionId: textListQuestionWhichIncreasesNestedRosterSizeId),

                Create.Entity.Roster(rosterId: parentRosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId: nestedRosterGroupId, rosterSizeQuestionId: textListQuestionWhichIncreasesNestedRosterSizeId),
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new TextListQuestionAnswered(userId, textListQuestionWhichIncreasesNestedRosterSizeId, new decimal[0],
                DateTime.Now, new[] { new Tuple<decimal, string>(40, "t1") }));
            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event_for_first_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event_for_first_nested_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 40 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        [NUnit.Framework.Test] public void should_raise_RosterRowsTitleChanged_event_for_first_nested_row () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                =>@event.ChangedInstances.Count(row =>
                row.Title == "t1" && row.RosterInstance.GroupId == nestedRosterGroupId && row.RosterInstance.RosterInstanceId == 40 &&
                    row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })) == 1);

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesRemoved_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid textListQuestionWhichIncreasesNestedRosterSizeId;
        private static Guid nestedRosterGroupId;
        private static Guid parentRosterGroupId;
    }
}
