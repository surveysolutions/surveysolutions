using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_real_question_which_is_roster_title_for_2_rosters_and_roster_level_is_1_and_answer_is_1_with_a_half : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            emptyRosterVector = new decimal[] { };
            var rosterInstanceId = (decimal)0;
            rosterVector = emptyRosterVector.Concat(new[] { rosterInstanceId }).ToArray();

            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: numericQuestionId),
                Create.Entity.Roster(rosterId: rosterAId, rosterSizeQuestionId:numericQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterTitleQuestionId: questionId, children: new IComposite[]
                {
                    Create.Entity.NumericRealQuestion(id: questionId),
                }),
                Create.Entity.Roster(rosterId: rosterBId, rosterSizeQuestionId:numericQuestionId, rosterTitleQuestionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(interview.EventSourceId, userId, numericQuestionId, 1));
            
            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerNumericRealQuestion(userId, questionId, rosterVector, DateTime.Now, 1.5);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_NumericRealQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<NumericRealQuestionAnswered>();

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

        [NUnit.Framework.Test] public void should_set_title_to_1_dot_5_in_all_RosterRowTitleChanged_events () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .Should().OnlyContain(@event => @event.ChangedInstances.All(x => x.Title == "1.5"));

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
