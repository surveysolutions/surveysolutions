using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_single_option_question_which_links_to_real_question_and_which_is_roster_title_for_2_rosters_and_roster_level_is_1
        : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();

            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            emptyRosterVector = new decimal[] { };
            var rosterInstanceId = 0m;
            rosterVector = emptyRosterVector.Concat(new[] { rosterInstanceId }).ToArray();

            questionId = Guid.Parse("11111111111111111111111111111111");
            var linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
            var linkedToRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var linkedOption1Vector = new decimal[] { 0 };
            linkedOption2Vector = new decimal[] { 1 };
            var linkedOption3Vector = new decimal[] { 2 };
            var linkedOption1Answer = 6.5;
            var linkedOption2Answer = 3.54;
            var linkedOption3Answer = -11.2;
            linkedOption2TextInvariantCulture = "3.54";

            var triggerQuestionId = Guid.NewGuid();
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Abc.Create.Entity.NumericRoster(rosterId: rosterAId, rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: questionId, variable: "ros1", children: new IComposite[]
                {
                    Abc.Create.Entity.SingleQuestion(id: questionId, linkedToQuestionId: linkedToQuestionId, variable: "link_single")
                }),
                Abc.Create.Entity.NumericRoster(rosterId: rosterBId, rosterSizeQuestionId: triggerQuestionId, variable: "ros2", rosterTitleQuestionId: questionId),
                Abc.Create.Entity.FixedRoster(rosterId: linkedToRosterId, variable: "ros3", fixedTitles: new [] { IntegrationCreate.FixedTitle(0), IntegrationCreate.FixedTitle(1), IntegrationCreate.FixedTitle(2)}, children: new IComposite[]
                {
                    Abc.Create.Entity.NumericRealQuestion(id: linkedToQuestionId, variable: "link_source"),
                })
            });

            interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument: questionnaireDocument);
            interview.AnswerNumericRealQuestion(userId, linkedToQuestionId, linkedOption1Vector, DateTime.Now, linkedOption1Answer);
            interview.AnswerNumericRealQuestion(userId, linkedToQuestionId, linkedOption2Vector, DateTime.Now, linkedOption2Answer);
            interview.AnswerNumericRealQuestion(userId, linkedToQuestionId, linkedOption3Vector, DateTime.Now, linkedOption3Answer);
            interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, 1);
            eventContext = new EventContext();

            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerSingleOptionLinkedQuestion(userId, questionId, rosterVector, DateTime.Now, linkedOption2Vector);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
            appDomainContext.Dispose();
        }

        [NUnit.Framework.Test] public void should_raise_SingleOptionLinkedQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        [NUnit.Framework.Test] public void should_raise_1_RosterRowsTitleChanged_events () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        [NUnit.Framework.Test] public void should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .Should().BeEquivalentTo(new []{rosterAId, rosterBId});

        [NUnit.Framework.Test] public void should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(emptyRosterVector)));

        [NUnit.Framework.Test] public void should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        [NUnit.Framework.Test] public void should_set_title_to_invariant_culture_formatted_value_assigned_to_corresponding_linked_to_question_in_all_RosterRowTitleChanged_events () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(x => x.Title))
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
