using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_single_option_question_which_links_to_integer_question_and_which_is_roster_title_for_2_rosters_and_roster_level_is_1 : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            emptyRosterVector = new decimal[] { };
            var rosterInstanceId = 0m;
            rosterVector = emptyRosterVector.Concat(new[] {rosterInstanceId}).ToArray();

            questionId = Guid.Parse("11111111111111111111111111111111");
            var linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
            var linkedToRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var linkedOption1Vector = new decimal[] {0};
            linkedOption2Vector = new decimal[] {1};
            var linkedOption3Vector = new decimal[] {2};
            int linkedOption1Answer = 6;
            int linkedOption2Answer = 30000;
            int linkedOption3Answer = -11;
            linkedOption2TextInvariantCulture = "30000";

            var triggerQuestionId = Guid.NewGuid();
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                    Abc.Create.Entity.Roster(rosterId: rosterAId, rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: questionId, variable: "ros1",
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.SingleQuestion(id: questionId, linkedToQuestionId: linkedToQuestionId,
                                variable: "link_single")
                        }),
                    Abc.Create.Entity.Roster(rosterId: rosterBId, rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: triggerQuestionId, variable: "ros2", rosterTitleQuestionId: questionId),
                    Abc.Create.Entity.Roster(rosterId: linkedToRosterId, variable: "ros3",
                        fixedRosterTitles: new[]
                        {
                            IntegrationCreate.FixedTitle(0), IntegrationCreate.FixedTitle(1),
                            IntegrationCreate.FixedTitle(2)
                        },
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(id: linkedToQuestionId, variable: "link_source"),
                        })
                });
            appDomainContext = AppDomainContext.Create();

            interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument);

            interview.AnswerNumericIntegerQuestion(userId, linkedToQuestionId, linkedOption1Vector, DateTime.Now,
                linkedOption1Answer);
            interview.AnswerNumericIntegerQuestion(userId, linkedToQuestionId, linkedOption2Vector, DateTime.Now,
                linkedOption2Answer);
            interview.AnswerNumericIntegerQuestion(userId, linkedToQuestionId, linkedOption3Vector, DateTime.Now,
                linkedOption3Answer);
            interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, 1);

            eventContext = new EventContext();

            interview.AnswerSingleOptionLinkedQuestion(userId, questionId, rosterVector, DateTime.Now,
                linkedOption2Vector);
        }

        [OneTimeTearDown]
        public void Down()
        {
            eventContext.Dispose();
            appDomainContext.Dispose();
        }

        [Test]
        public void should_raise_SingleOptionLinkedQuestionAnswered_event() =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        [Test]
        public void should_raise_1_RosterRowsTitleChanged_events() =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        [Test]
        public void should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .Should().BeEquivalentTo(rosterAId, rosterBId);

        [Test]
        public void should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(
                @event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(Array.Empty<decimal>())));

        [Test]
        public void should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events  () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        [Test]
        public void should_set_title_to_invariant_culture_formatted_value_assigned_to_corresponding_linked_to_question_in_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .SelectMany(@event => @event.ChangedInstances.Select(x=>x.Title))
                .Should().OnlyContain(title => title == linkedOption2TextInvariantCulture);

        private static AppDomainContext appDomainContext;
        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] rosterVector;
        private static decimal[] emptyRosterVector;
        private static Guid rosterAId;
        private static Guid rosterBId;
        private static decimal[] linkedOption2Vector;
        private static string linkedOption2TextInvariantCulture;
    }
}
