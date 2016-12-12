using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_multiple_options_question_which_is_roster_title_for_2_rosters_and_roster_level_is_1_and_options_are_X_Y_Z_and_selected_are_Z_and_X : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var rosterInstanceId = 0m;
            rosterVector = ((decimal[]) RosterVector.Empty).Concat(new[] { rosterInstanceId }).ToArray();

            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            optionX = 1;
            optionY = -2;
            optionZ = 2;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(numericId),
                Create.Entity.Roster(rosterAId, rosterSizeQuestionId:numericId, rosterTitleQuestionId: questionId, children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(questionId: questionId, textAnswers: new []
                    {
                        Create.Entity.Answer("X", optionX),
                        Create.Entity.Answer("Y", optionY),
                        Create.Entity.Answer("Z", optionZ),
                    }),
                }),
                Create.Entity.Roster(rosterBId, rosterSizeQuestionId:numericId, rosterTitleQuestionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(numericId, RosterVector.Empty, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, RosterVector.Empty, rosterInstanceId, sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, RosterVector.Empty, rosterInstanceId, sortIndex: null));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerMultipleOptionsQuestion(userId, questionId, rosterVector, DateTime.Now, new[] { optionZ, optionX });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<MultipleOptionsQuestionAnswered>();

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .ShouldContainOnly(rosterAId, rosterBId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual((decimal[]) RosterVector.Empty)));

        It should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        It should_set_title_to__Z_comma_space_X__in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.Title == "Z, X"));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] rosterVector;
        private static Guid rosterAId;
        private static Guid rosterBId;
        private static int optionX;
        private static int optionY;
        private static int optionZ;
        private static Guid numericId = Guid.Parse("22222222222222222222222222222222");
    }
}