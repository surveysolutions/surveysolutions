using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_fixed_roster_with_3_fixed_titles : InterviewTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            answersTime = new DateTime(2013, 09, 01);
            var fixedRosterTitles = new[] { new FixedRosterTitle(0, "Title 1"), new FixedRosterTitle(1, "Title 2"), new FixedRosterTitle(2, "Title 3") };
            fixedRosterId = Guid.Parse("a7b0d842-0355-4eab-a943-968c9c013d97");

            Guid mandatoryQuestionId = Guid.Parse("33330000FFFFFFFFFFFFFFFFFFFF5555"); 

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetFixedRosterGroups(null) == new Guid[] {fixedRosterId}
                && _.GetFixedRosterTitles(fixedRosterId) == fixedRosterTitles
                && _.IsRosterGroup(fixedRosterId) == true
                && _.GetRostersFromTopToSpecifiedGroup(fixedRosterId) == new Guid[] { fixedRosterId }
                );

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
            SetupInstanceToMockedServiceLocator<IInterviewExpressionStatePrototypeProvider>(
              CreateInterviewExpressionStateProviderStub());
            eventContext = new EventContext();
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, 1, answersToFeaturedQuestions, answersTime, supervisorId);

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>();

        It should_raise_RosterInstancesAdded_event_with_3_instances = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(3);

        It should_set_roster_id_to_all_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.GroupId == fixedRosterId);

        It should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.OuterRosterVector.Length == 0);

        It should_set__0__or__1__or_2__as_roster_instance_ids_in_RosterInstancesAdded_event = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Select(instance => instance.RosterInstanceId).ToArray()
                .ShouldContainOnly(0, 1, 2);

        It should_set_null_in_sort_index_to_all_instances_in_RosterInstancesAdded_event = () =>
             eventContext.GetEvent<RosterInstancesAdded>().Instances
                .ShouldEachConformTo(instance => instance.SortIndex == null);

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_set_roster_id_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.GroupId == fixedRosterId));

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.Length == 0));

        It should__0__or__1__or_2__as_roster_instance_ids_in_RosterRowsTitleChanged_event = () =>
          eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.RosterInstanceId)).ToArray()
              .ShouldContainOnly(0, 1, 2);

        It should_set_title__Title_1__in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_0 = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 0 && row.Title == "Title 1") == 1);

        It should_set_title__Title_2__in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_1 = () =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 1 && row.Title == "Title 2") == 1);

        It should_set_title__Title_3__in_RosterRowsTitleChanged_event_with_roster_instance_id_equal_to_2 = () =>
             eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(
                @event => @event.ChangedInstances.Count(row => row.RosterInstance.RosterInstanceId == 2 && row.Title == "Title 3") == 1);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid fixedRosterId;
    }
}