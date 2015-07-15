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
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_text_list_question_which_is_roster_size_for_2_rosters_with_previous_answer_and_new_answer_has_only_chaned_values : InterviewTestsContext
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
                    && _.GetMaxRosterRowCount() == Constants.MaxRosterRowCount
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
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, emptyRosterVector, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, emptyRosterVector, 2));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, emptyRosterVector, 3));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, emptyRosterVector, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, emptyRosterVector, 2));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, emptyRosterVector, 3));

            eventContext = new EventContext();
        };

        private Because of = () =>
            interview.AnswerTextListQuestion(userId, textListQuestionId, emptyRosterVector, DateTime.Now,
                new[]
                {
                    new Tuple<decimal, string>(1, "Answer 1 !New"),
                    new Tuple<decimal, string>(2, "Answer 2 !New"),
                    new Tuple<decimal, string>(3, "Answer 3 !New")
                });

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
           eventContext.ShouldContainEvent<TextListQuestionAnswered>();

        It should_raise_0_RosterRowAdded_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesAdded>(count: 0);

        It should_raise_0_any_RosterRowRemoved_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesAdded>(count: 0);

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_raise_RosterRowsTitleChanged_event_with_2_roster_instance_id_equals_to_1 = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 1) == 2);

        It should_raise_RosterRowsTitleChanged_event_with_2_roster_instance_id_equals_to_2 = () =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 2) == 2);

        It should_raise_RosterRowsTitleChanged_event_with_2_roster_instance_id_equals_to_3 = () =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 3) == 2);

        It should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .ShouldContain(rosterAId, rosterBId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(emptyRosterVector)));

        It should_set_title_to__Answer_1_New__in_all_RosterRowTitleChanged_events_with_roster_instance_id_equals_to_1 = () =>
            eventContext.GetSingleEvent<RosterInstancesTitleChanged>()
                        .ChangedInstances
                        .Where(x => x.RosterInstance.RosterInstanceId == 1)
                        .Select(x => x.Title)
                        .ShouldContainOnly("Answer 1 !New", "Answer 1 !New");

        It should_set_title_to__Answer_2_New__in_all_RosterRowTitleChanged_events_with_roster_instance_id_equals_to_2 = () =>
            eventContext.GetSingleEvent<RosterInstancesTitleChanged>()
                        .ChangedInstances
                        .Where(x => x.RosterInstance.RosterInstanceId == 2)
                        .Select(x => x.Title)
                        .ShouldContainOnly("Answer 2 !New", "Answer 2 !New");

        It should_set_title_to__Answer_3_New__in_all_RosterRowTitleChanged_events_with_roster_instance_id_equals_to_3 = () =>
            eventContext.GetSingleEvent<RosterInstancesTitleChanged>()
                        .ChangedInstances
                        .Where(x => x.RosterInstance.RosterInstanceId == 3)
                        .Select(x => x.Title)
                        .ShouldContainOnly("Answer 3 !New", "Answer 3 !New");

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
            new Tuple<decimal, string>(3, "Answer 3")
        };
    }
}