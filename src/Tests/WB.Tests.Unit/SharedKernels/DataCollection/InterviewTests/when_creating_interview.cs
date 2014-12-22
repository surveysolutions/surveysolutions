﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();

            interviewId = Guid.Parse("11000000000000000000000000000000");
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            questionnaireVersion = 18;
            answersToFeaturedQuestions = new Dictionary<Guid, object>();

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, 1, answersToFeaturedQuestions, DateTime.Now, responsibleSupervisorId);

        It should_raise_InterviewCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        It should_provide_questionnaire_id_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewCreated>()
                .QuestionnaireId.ShouldEqual(questionnaireId);

        It should_provide_questionnaire_verstion_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewCreated>()
                .QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static long questionnaireVersion;
        private static Guid userId;
        private static Guid responsibleSupervisorId;
        private static Guid interviewId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
    }
}