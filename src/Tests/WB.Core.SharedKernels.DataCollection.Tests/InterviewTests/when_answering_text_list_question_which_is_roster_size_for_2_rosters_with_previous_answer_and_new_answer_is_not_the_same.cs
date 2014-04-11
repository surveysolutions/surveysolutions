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
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_text_list_question_which_is_roster_size_for_2_rosters_with_previous_answer_and_new_answer_is_not_the_same : InterviewTestsContext
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
            interview.Apply(new RosterRowAdded(rosterAId, emptyRosterVector, 1, sortIndex: 1));
            interview.Apply(new RosterRowAdded(rosterAId, emptyRosterVector, 2, sortIndex: 2));
            interview.Apply(new RosterRowAdded(rosterAId, emptyRosterVector, 3, sortIndex: 3));
            interview.Apply(new RosterRowAdded(rosterBId, emptyRosterVector, 1, sortIndex: 1));
            interview.Apply(new RosterRowAdded(rosterBId, emptyRosterVector, 2, sortIndex: 2));
            interview.Apply(new RosterRowAdded(rosterBId, emptyRosterVector, 3, sortIndex: 3));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerTextListQuestion(userId, textListQuestionId, emptyRosterVector, DateTime.Now,
                new[]
                {
                    new Tuple<decimal, string>(1, "Answer 1"),
                    new Tuple<decimal, string>(3, "Answer 3"),
                    new Tuple<decimal, string>(5, "Answer 4")
                });

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<TextListQuestionAnswered>();

        It should_raise_RosterInstancesAdded_event_with_2_instances = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(2);

        It should_raise_RosterInstancesRemoved_event_with_2_instances = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count().ShouldEqual(2);

        It should_raise_RosterInstancesAdded_event_with_1_instance_with_GroupId_equals_to_rosterAId = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count(instance => instance.GroupId == rosterAId).ShouldEqual(1);

        It should_raise_RosterInstancesAdded_event_with_1_instance_with_GroupId_equals_to_rosterBId = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count(instance => instance.GroupId == rosterBId).ShouldEqual(1);

        It should_raise_RosterInstancesRemoved_event_with_1_instance_with_GroupId_equals_to_rosterAId = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == rosterAId).ShouldEqual(1);

        It should_raise_RosterInstancesRemoved_event_with_1_instance_with_GroupId_equals_to_rosterBId = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == rosterBId).ShouldEqual(1);

        It should_raise_RosterInstancesAdded_event_with_2_instances_with_roster_instance_id_equals_to_5 = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count(instance => instance.RosterInstanceId == 5).ShouldEqual(2);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_with_roster_instance_id_equals_to_2 = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.RosterInstanceId == 2).ShouldEqual(2);

        It should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.OuterRosterVector.Length == 0);

        It should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesRemoved_event = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances
                .ShouldEachConformTo(instance => instance.OuterRosterVector.Length == 0);

        It should_set_not_null_in_sort_index_to_all_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.SortIndex != null);

        It should_raise_RosterInstancesAdded_event_with_2_instances_with_sort_index_equals_to_5 = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count(instance => instance.SortIndex == 5).ShouldEqual(2);

        It should_raise_2_RosterRowTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterRowTitleChanged>(count: 2);

        It should_raise_2_RosterRowTitleChanged_events_with_roster_instance_id_equals_to_5 = () =>
            eventContext.GetEvents<RosterRowTitleChanged>().Where(@event => @event.RosterInstanceId == 5).Count().ShouldEqual(2);

        It should_set_2_affected_roster_ids_in_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterRowTitleChanged>().Select(@event => @event.GroupId).Distinct().ToArray()
                .ShouldContainOnly(rosterAId, rosterBId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterRowTitleChanged>()
                .ShouldEachConformTo(@event => Enumerable.SequenceEqual(@event.OuterRosterVector, emptyRosterVector));

        It should_set_title_to__Answer_4__in_all_RosterRowTitleChanged_events_with_roster_instance_id_equals_to_5 = () =>
            eventContext.GetEvents<RosterRowTitleChanged>().Where(@event => @event.RosterInstanceId == 5).Select(@event => @event.Title)
                .ShouldEachConformTo(title => title == "Answer 4");

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