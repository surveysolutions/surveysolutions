﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
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
            var fixedRosterTitles = new string[] { "Title 1", "Title 2", "Title 3" };
            fixedRosterId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");

            Guid mandatoryQuestionId = Guid.Parse("33330000FFFFFFFFFFFFFFFFFFFF5555"); 

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetFixedRosterGroups() == new Guid[] {fixedRosterId}
                && _.GetFixedRosterTitles(fixedRosterId) == fixedRosterTitles
                && _.IsRosterGroup(fixedRosterId) == true
                && _.GetRostersFromTopToSpecifiedGroup(fixedRosterId) == new Guid[] { fixedRosterId }
                );

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, answersToFeaturedQuestions, answersTime, supervisorId);

        It should_not_raise_RosterRowRemoved_event = () =>
        eventContext.ShouldNotContainEvent<RosterRowRemoved>();

        It should_raise_2_RosterRowAdded_events = () =>
            eventContext.ShouldContainEvents<RosterRowAdded>(count: 3);

        It should_set_roster_id_to_all_RosterRowAdded_events = () =>
            eventContext.GetEvents<RosterRowAdded>()
                .ShouldEachConformTo(@event => @event.GroupId == fixedRosterId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowAdded_events = () =>
            eventContext.GetEvents<RosterRowAdded>()
                .ShouldEachConformTo(@event => @event.OuterRosterVector.Length == 0);

        It should_set__0__or__1__or_2__as_roster_instance_ids_in_RosterRowAdded_events = () =>
            eventContext.GetEvents<RosterRowAdded>().Select(@event => @event.RosterInstanceId).ToArray()
                .ShouldContainOnly(0, 1, 2);

        It should_set_null_in_sort_index_to_all_RosterRowAdded_events = () =>
             eventContext.GetEvents<RosterRowAdded>()
                .ShouldEachConformTo(@event => @event.SortIndex == null);
       
        It should_raise_3_RosterRowTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterRowTitleChanged>(count: 3);

        It should_set_roster_id_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterRowTitleChanged>()
                .ShouldEachConformTo(@event => @event.GroupId == fixedRosterId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterRowTitleChanged>()
                .ShouldEachConformTo(@event => @event.OuterRosterVector.Length == 0);

        It should__0__or__1__or_2__as_roster_instance_ids_in_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterRowTitleChanged>().Select(@event => @event.RosterInstanceId).ToArray()
                .ShouldContainOnly(0, 1, 2);

        It should_set_title__Title_1__in_RosterRowTitleChanged_event_with_roster_instance_id_equal_to_0 = () =>
            eventContext.GetEvents<RosterRowTitleChanged>().Single(@event => @event.RosterInstanceId == 0)
                .Title.ShouldEqual("Title 1");

        It should_set_title__Title_2__in_RosterRowTitleChanged_event_with_roster_instance_id_equal_to_1 = () =>
           eventContext.GetEvents<RosterRowTitleChanged>().Single(@event => @event.RosterInstanceId == 1)
               .Title.ShouldEqual("Title 2");

        It should_set_title__Title_3__in_RosterRowTitleChanged_event_with_roster_instance_id_equal_to_2 = () =>
           eventContext.GetEvents<RosterRowTitleChanged>().Single(@event => @event.RosterInstanceId == 2)
               .Title.ShouldEqual("Title 3");

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