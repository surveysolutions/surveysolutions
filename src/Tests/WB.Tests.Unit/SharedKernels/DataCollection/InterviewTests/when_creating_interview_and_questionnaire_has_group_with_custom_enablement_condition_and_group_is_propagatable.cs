﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_group_with_custom_enablement_condition_and_group_is_propagatable : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            answersTime = new DateTime(2013, 09, 01);

            Guid groupId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");

            var questionaire = Mock.Of<IQuestionnaire>(_ => _.IsRosterGroup(groupId) == true);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            Create.Interview().CreateInterview(questionnaireId, 1, supervisorId, answersToFeaturedQuestions, answersTime, userId);

        It should_not_raise_GroupDisabled_event = () =>
            eventContext.ShouldNotContainEvent<GroupsDisabled>();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        static EventContext eventContext;
        static Guid userId;
        static Guid questionnaireId;
        static Dictionary<Guid, object> answersToFeaturedQuestions;
        static DateTime answersTime;
        static Guid supervisorId;
    }
}