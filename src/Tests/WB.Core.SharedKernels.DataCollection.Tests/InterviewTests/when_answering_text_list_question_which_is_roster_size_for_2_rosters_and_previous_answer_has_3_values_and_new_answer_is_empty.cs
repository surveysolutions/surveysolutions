using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_text_list_question_which_is_roster_size_for_2_rosters_and_previous_answer_has_3_values_and_new_answer_is_empty : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var questionaire = Mock.Of<IQuestionnaire>(_

                => _.HasQuestion(textListQuestionId) == true
                    && _.GetQuestionType(textListQuestionId) == QuestionType.TextList
                    && _.ShouldQuestionSpecifyRosterSize(textListQuestionId) == true
                    && _.GetListSizeForListQuestion(textListQuestionId) == 10
                    && _.GetRosterGroupsByRosterSizeQuestion(textListQuestionId) == new[] { rosterAId, rosterBId }

                    && _.HasGroup(rosterAId) == true
                    && _.HasGroup(rosterBId) == true
                    && _.GetAllUnderlyingQuestions(rosterAId) == new Guid[0]
                    && _.GetAllUnderlyingQuestions(rosterAId) == new Guid[0]
                );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new TextListQuestionAnswered(userId, textListQuestionId, new decimal[] { }, DateTime.Now, previousAnswer));
            interview.Apply(new RosterRowAdded(rosterAId, emptyRosterVector, 1, sortIndex: null));
            interview.Apply(new RosterRowAdded(rosterAId, emptyRosterVector, 2, sortIndex: null));
            interview.Apply(new RosterRowAdded(rosterAId, emptyRosterVector, 3, sortIndex: null));
            interview.Apply(new RosterRowAdded(rosterBId, emptyRosterVector, 1, sortIndex: null));
            interview.Apply(new RosterRowAdded(rosterBId, emptyRosterVector, 2, sortIndex: null));
            interview.Apply(new RosterRowAdded(rosterBId, emptyRosterVector, 3, sortIndex: null));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerTextListQuestion(userId, textListQuestionId, emptyRosterVector, DateTime.Now, new Tuple<decimal, string>[0]);

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<TextListQuestionAnswered>();

        It should_not_raise_any_RosterRowRemoved_events = () =>
            eventContext.ShouldContainEvents<RosterRowRemoved>(count: 6);

        It should_raise_3_RosterRowRemoved_events_with_GroupId_equals_to_rosterAId = () =>
            eventContext.GetEvents<RosterRowRemoved>().Where(@event => @event.GroupId == rosterAId).Count().ShouldEqual(3);

        It should_raise_3_RosterRowRemoved_events_with_GroupId_equals_to_rosterBId = () =>
            eventContext.GetEvents<RosterRowRemoved>().Where(@event => @event.GroupId == rosterBId).Count().ShouldEqual(3);

        It should_raise_2_RosterRowRemoved_events_with_roster_instance_id_equals_to_1 = () =>
            eventContext.GetEvents<RosterRowRemoved>().Where(@event => @event.RosterInstanceId == 1).Count().ShouldEqual(2);

        It should_raise_2_RosterRowRemoved_events_with_roster_instance_id_equals_to_2 = () =>
            eventContext.GetEvents<RosterRowRemoved>().Where(@event => @event.RosterInstanceId == 2).Count().ShouldEqual(2);

        It should_raise_2_RosterRowRemoved_events_with_roster_instance_id_equals_to_3 = () =>
            eventContext.GetEvents<RosterRowRemoved>().Where(@event => @event.RosterInstanceId == 3).Count().ShouldEqual(2);

        It should_set_empty_outer_roster_vector_to_all_RosterRowRemoved_events = () =>
            eventContext.GetEvents<RosterRowRemoved>()
                .ShouldEachConformTo(@event => Enumerable.SequenceEqual(@event.OuterRosterVector, emptyRosterVector));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid propagatedQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid textListQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] emptyRosterVector = new decimal[] { };
        private static Guid rosterAId = Guid.Parse("00000000000000003333333333333333");
        private static Guid rosterBId = Guid.Parse("00000000000000004444444444444444");
        private static Tuple<decimal, string>[] previousAnswer = new[]
            {
                new Tuple<decimal, string>(1, "Answer 1"),
                new Tuple<decimal, string>(2, "Answer 2"),
                new Tuple<decimal, string>(3, "Answer 3"),
            };
    }
}