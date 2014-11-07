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
            interview.Apply(Create.Events.RosterInstancesAdded(rosterAId, emptyRosterVector, 1, sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterAId, emptyRosterVector, 2, sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterAId, emptyRosterVector, 3, sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterBId, emptyRosterVector, 1, sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterBId, emptyRosterVector, 2, sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterBId, emptyRosterVector, 3, sortIndex: null));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerTextListQuestion(userId, textListQuestionId, emptyRosterVector, DateTime.Now, new Tuple<decimal, string>[0]);

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<TextListQuestionAnswered>();

        It should_raise_RosterInstancesRemoved_event_with_6_instances = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count().ShouldEqual(6);

        It should_raise_RosterInstancesRemoved_event_with_3_instances_where_GroupId_equals_to_rosterAId = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == rosterAId).ShouldEqual(3);

        It should_raise_RosterInstancesRemoved_event_with_3_instances_where_GroupId_equals_to_rosterBId = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == rosterBId).ShouldEqual(3);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_where_roster_instance_id_equals_to_1 = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.RosterInstanceId == 1).ShouldEqual(2);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_where_roster_instance_id_equals_to_2 = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.RosterInstanceId == 2).ShouldEqual(2);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_where_roster_instance_id_equals_to_3 = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.RosterInstanceId == 3).ShouldEqual(2);

        It should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesRemoved_event = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances
                .ShouldEachConformTo(instance => instance.OuterRosterVector.Length == 0);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Interview interview;
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