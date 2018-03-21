using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_with_nested_fixed_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            fixedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            rosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),

                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, children: new IComposite[]
                {
                    Create.Entity.FixedRoster(rosterId: fixedRosterGroupId, fixedTitles: new []
                    {
                        Create.Entity.FixedTitle(0, title1),
                        Create.Entity.FixedTitle(1, title2),
                    }),
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

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
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 2);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_for_first_row_of_first_level_roster () =>
          eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_for_second_row_of_first_level_roster () =>
          eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 1 && instance.OuterRosterVector.Length == 0));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_for_first_row_of_fixed_roster_by_first_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == fixedRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_for_first_row_of_fixed_roster_by_second_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == fixedRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 1 })));

        [NUnit.Framework.Test] public void should_rise_RosterRowsTitleChanged_event_with_title_of_first_row_of_fixed_roster_by_first_row () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row =>
                    row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 0 && row.RosterInstance.OuterRosterVector.Length == 1 &&
                        row.RosterInstance.OuterRosterVector[0] == 0 && row.Title == title1));

        [NUnit.Framework.Test] public void should_rise_RosterRowsTitleChanged_event_with_title_of_first_row_of_fixed_roster_by_second_row () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row =>
               row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 0 && row.RosterInstance.OuterRosterVector.Length == 1 && row.RosterInstance.OuterRosterVector[0] == 0 && row.Title == title1));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_for_second_row_of_fixed_roster_by_first_row () =>
           eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == fixedRosterGroupId && instance.RosterInstanceId == 1 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_for_second_row_of_fixed_roster_by_second_row () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == fixedRosterGroupId && instance.RosterInstanceId == 1 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 1 })));

        [NUnit.Framework.Test] public void should_rise_RosterRowsTitleChanged_event_with_title_of_second_row_of_fixed_roster_by_first_row () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row
                => row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 1 && row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 }) && row.Title == title2));

        [NUnit.Framework.Test] public void should_rise_RosterRowsTitleChanged_event_with_title_of_second_row_of_fixed_roster_by_second_row () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row
                => row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 1 && row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 1 }) && row.Title == title2));

        [NUnit.Framework.Test] public void should_raise_RosterRowsTitleChanged_event_with_title_of_first_nested_row () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
                => @event.ChangedInstances.Any(row
                =>
                row.Title == "t1" && row.RosterInstance.GroupId == fixedRosterGroupId && row.RosterInstance.RosterInstanceId == 0 &&
                    row.RosterInstance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid fixedRosterGroupId;
        private static Guid rosterGroupId;
        private static string title1 = "t1";
        private static string title2 = "t2";
    }
}
