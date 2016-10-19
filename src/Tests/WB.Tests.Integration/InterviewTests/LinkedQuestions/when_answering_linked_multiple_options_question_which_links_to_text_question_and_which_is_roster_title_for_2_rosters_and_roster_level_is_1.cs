using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_multiple_options_question_which_links_to_text_question_and_which_is_roster_title_for_2_rosters_and_roster_level_is_1 : InterviewTestsContext
    {
        Establish context = () =>
        {
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
            linkedOption3Vector = new decimal[] { 2 };
            var linkedOption1Text = "linked option 1";
            linkedOption2Text = "linked option 2";
            linkedOption3Text = "linked option 3";

            var triggerQuestionId= Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Integration.Create.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Create.Roster(id: rosterAId, rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: questionId, variable: "ros1",
                    children: new IComposite[]
                    {
                        Create.MultyOptionsQuestion(id: questionId, linkedToQuestionId: linkedToQuestionId,
                            variable: "link_multi")
                    }),
                Create.Roster(id: rosterBId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: triggerQuestionId, variable: "ros2", rosterTitleQuestionId: questionId),
                Create.Roster(id: linkedToRosterId, variable: "ros3", fixedRosterTitles: new [] { Create.FixedRosterTitle(0), Create.FixedRosterTitle(1), Create.FixedRosterTitle(2)},
                    children: new IComposite[]
                    {
                        Create.TextQuestion(id: linkedToQuestionId, variable: "link_source"),
                    })
            });

            interview = SetupInterview(questionnaireDocument: questionnaireDocument);

            interview.AnswerTextQuestion(userId, linkedToQuestionId, linkedOption1Vector, DateTime.Now, linkedOption1Text);
            interview.AnswerTextQuestion(userId, linkedToQuestionId, linkedOption2Vector, DateTime.Now, linkedOption2Text);
            interview.AnswerTextQuestion(userId, linkedToQuestionId, linkedOption3Vector, DateTime.Now, linkedOption3Text);
            interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, 1);

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
            interview.AnswerMultipleOptionsLinkedQuestion(userId, questionId, rosterVector, DateTime.Now, new [] { linkedOption3Vector, linkedOption2Vector });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultipleOptionsLinkedQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<MultipleOptionsLinkedQuestionAnswered>();

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

        It should_set_title_to_text_assigned_to_corresponding_linked_to_questions_separated_by_comma_and_space_in_answer_order_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(x => x.Title))
                .ShouldEachConformTo(title => title == linkedOption3Text + ", " + linkedOption2Text);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] rosterVector;
        private static decimal[] emptyRosterVector;
        private static Guid rosterAId;
        private static Guid rosterBId;
        private static decimal[] linkedOption2Vector;
        private static string linkedOption2Text;
        private static decimal[] linkedOption3Vector;
        private static string linkedOption3Text;
    }
}