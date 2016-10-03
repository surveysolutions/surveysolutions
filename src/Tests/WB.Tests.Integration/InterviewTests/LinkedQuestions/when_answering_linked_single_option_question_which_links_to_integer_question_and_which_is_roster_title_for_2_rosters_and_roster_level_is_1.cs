using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_single_option_question_which_links_to_integer_question_and_which_is_roster_title_for_2_rosters_and_roster_level_is_1 : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            emptyRosterVector = new decimal[] { };
            var rosterInstanceId = (decimal)22.5;
            rosterVector = emptyRosterVector.Concat(new[] { rosterInstanceId }).ToArray();

            questionId = Guid.Parse("11111111111111111111111111111111");
            var linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
            var linkedToRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var linkedOption1Vector = new decimal[] { 0 };
            linkedOption2Vector = new decimal[] { 1 };
            var linkedOption3Vector = new decimal[] { 2 };
            int linkedOption1Answer = 6;
            int linkedOption2Answer = 30000;
            int linkedOption3Answer = -11;
            linkedOption2TextInvariantCulture = "30000";

            var triggerQuestionId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Integration.Create.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Create.Roster(id: rosterAId, rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: questionId, variable: "ros1",
                    children: new IComposite[]
                    {
                        Create.SingleQuestion(id: questionId, linkedToQuestionId: linkedToQuestionId,
                            variable: "link_single")
                    }),
                Create.Roster(id: rosterBId, rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, variable: "ros2", rosterTitleQuestionId: questionId),
                Create.Roster(id: linkedToRosterId, variable: "ros3",
                    children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: linkedToQuestionId, variable: "link_source"),
                    })
            });

            interview = SetupInterview(questionnaireDocument: questionnaireDocument);
            interview.Apply(Create.Event.RosterInstancesAdded(linkedToRosterId, emptyRosterVector, linkedOption1Vector[0], sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(linkedToRosterId, emptyRosterVector, linkedOption2Vector[0], sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(linkedToRosterId, emptyRosterVector, linkedOption3Vector[0], sortIndex: null));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, linkedToQuestionId, linkedOption1Vector, DateTime.Now, linkedOption1Answer));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, linkedToQuestionId, linkedOption2Vector, DateTime.Now, linkedOption2Answer));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, linkedToQuestionId, linkedOption3Vector, DateTime.Now, linkedOption3Answer));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, emptyRosterVector, rosterInstanceId, sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, emptyRosterVector, rosterInstanceId, sortIndex: null));
            interview.Apply(new LinkedOptionsChanged(new[]
            {
                new ChangedLinkedOptions(new Identity(questionId, rosterVector), new RosterVector[]
                {
                    linkedOption1Vector, linkedOption2Vector, linkedOption3Vector
                })
            }));
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerSingleOptionLinkedQuestion( userId, questionId, rosterVector, DateTime.Now, linkedOption2Vector);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionLinkedQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .ShouldContainOnly(rosterAId, rosterBId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(emptyRosterVector)));

        It should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        It should_set_title_to_invariant_culture_formatted_value_assigned_to_corresponding_linked_to_question_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .SelectMany(@event => @event.ChangedInstances.Select(x=>x.Title))
                .ShouldEachConformTo(title => title == linkedOption2TextInvariantCulture);

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