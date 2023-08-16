using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_integer_question_which_is_roster_title_for_2_rosters_and_roster_level_is_1_and_answer_is_7 : InterviewTestsContext
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
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");


            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: numericQuestionId),
                    Create.Entity.Roster(rosterId: rosterAId, rosterSizeQuestionId: numericQuestionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question, rosterTitleQuestionId: questionId,
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(id: questionId),
                        }),
                    Create.Entity.Roster(rosterId: rosterBId, rosterSizeQuestionId: numericQuestionId,
                        rosterTitleQuestionId: questionId),
                }));


            IQuestionnaireStorage questionnaireRepository =
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(numericQuestionId, emptyRosterVector, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, emptyRosterVector, rosterInstanceId,
                sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, emptyRosterVector, rosterInstanceId,
                sortIndex: null));

            eventContext = new EventContext();

            interview.AnswerNumericIntegerQuestion(userId, questionId, rosterVector, DateTime.Now, 7);
        }

        [Test]
        public void should_raise_NumericIntegerQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>();

        [Test]
        public void should_raise_1_RosterRowsTitleChanged_events() =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        [Test]
        public void should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .Should().BeEquivalentTo(new []{rosterAId, rosterBId});

        [Test]
        public void should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(emptyRosterVector)));

        [Test]
        public void should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        [Test]
        public void should_set_title_to__7__in_all_RosterRowTitleChanged_events() =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.Title == "7"));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] rosterVector;
        private static decimal[] emptyRosterVector;
        private static Guid rosterAId;
        private static Guid rosterBId;
        private static Guid numericQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}
