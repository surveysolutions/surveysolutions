using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
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

            option2Title = "Option 2 Title";
            option4Title = "Option 4 Title";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, textAnswers: new []
                {
                    Create.Entity.Answer("option 1", 147m),
                    Create.Entity.Answer(option2Title, option2 = 184),
                    Create.Entity.Answer("option 3", 3),
                    Create.Entity.Answer(option4Title, option4 = 1),
                    Create.Entity.Answer("option 5", 256128m),
                }),

                Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

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

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>();

        It should_raise_RosterInstancesAdded_event_with_2_instances = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(2);

        It should_set_roster_id_to_all_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.GroupId == rosterId);

        It should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.OuterRosterVector.Length == 0);

        It should_set_2nd_and_4th_options_as_roster_instance_ids_in_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Select(instance => instance.RosterInstanceId).ToArray()
                .ShouldContainOnly(option2, option4);

        It should_set_sort_index_to_1_in_RosterInstancesAdded_instance_with_roster_instance_id_equal_to_2nd_option = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Single(instance => instance.RosterInstanceId == option2)
                .SortIndex.ShouldEqual(0);

        It should_set_sort_index_to_3_in_RosterInstancesAdded_instance_with_roster_instance_id_equal_to_4th_option = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Single(instance => instance.RosterInstanceId == option4)
                .SortIndex.ShouldEqual(1);

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_set_roster_id_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.GroupId == rosterId));

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.Length == 0));

        It should_set_2nd_and_4th_options_as_roster_instance_ids_in_RosterRowsTitleChanged_events = () =>
           eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.RosterInstanceId)).ToArray()
               .ShouldContain(option2, option2);

        It should_set_title_to_2nd_option_title_in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_2nd_option = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == option2 && row.Title == option2Title) == 1);

        It should_set_title_to_4th_option_title_in_RostersRowTitleChanged_event_with_roster_instance_id_equal_to_4th_option = () =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == option4 && row.Title == option4Title) == 1);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] emptyRosterVector;
        private static int option2;
        private static int option4;
        private static Guid rosterId;
        private static string option2Title;
        private static string option4Title;
    }
}