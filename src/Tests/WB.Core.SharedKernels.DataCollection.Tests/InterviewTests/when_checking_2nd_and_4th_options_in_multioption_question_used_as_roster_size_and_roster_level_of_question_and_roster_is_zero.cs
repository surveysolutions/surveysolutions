using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_checking_2nd_and_4th_options_in_multioption_question_used_as_roster_size_and_roster_level_of_question_and_roster_is_zero : InterviewTestsContext
    {
        Establish context = () =>
        {
            emptyRosterVector = new decimal[] { };
            userId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("44444444444444444444444444444444");

            var availableOptions = new[]
            {
                (decimal) 14.7,
                option2 = (decimal) 18.4,
                3,
                option4 = -1,
                (decimal) 256.128,
            };

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(questionId) == true
                && _.GetQuestionType(questionId) == QuestionType.MultyOption
                && _.GetAnswerOptionsAsValues(questionId) == availableOptions
                && _.GetRosterGroupsByRosterSizeQuestion(questionId) == new[] { rosterId }
                && _.HasGroup(rosterId) == true
                && _.IsRosterGroup(rosterId) == true
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire));

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerMultipleOptionsQuestion(userId, questionId, emptyRosterVector, DateTime.Now, new[] { option2, option4 });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<MultipleOptionsQuestionAnswered>();

        It should_raise_2_RosterRowAdded_events = () =>
            eventContext.ShouldContainEvents<RosterRowAdded>(count: 2);

        It should_set_roster_id_to_all_RosterRowAdded_events = () =>
            eventContext.GetEvents<RosterRowAdded>()
                .ShouldEachConformTo(@event => @event.GroupId == rosterId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowAdded_events = () =>
            eventContext.GetEvents<RosterRowAdded>()
                .ShouldEachConformTo(@event => @event.OuterRosterVector.Length == 0);

        It should_set_2nd_and_4th_options_as_roster_instance_ids_in_RosterRowAdded_events = () =>
            eventContext.GetEvents<RosterRowAdded>().Select(@event => @event.RosterInstanceId).ToArray()
                .ShouldContainOnly(option2, option4);

        It should_set_sort_index_to_1_in_RosterRowAdded_event_with_roster_instance_id_equal_to_2nd_option = () =>
            eventContext.GetEvents<RosterRowAdded>().Single(@event => @event.RosterInstanceId == option2)
                .SortIndex.ShouldEqual(1);

        It should_set_sort_index_to_3_in_RosterRowAdded_event_with_roster_instance_id_equal_to_4th_option = () =>
            eventContext.GetEvents<RosterRowAdded>().Single(@event => @event.RosterInstanceId == option4)
                .SortIndex.ShouldEqual(3);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] emptyRosterVector;
        private static decimal option2;
        private static decimal option4;
        private static Guid rosterId;
    }
}