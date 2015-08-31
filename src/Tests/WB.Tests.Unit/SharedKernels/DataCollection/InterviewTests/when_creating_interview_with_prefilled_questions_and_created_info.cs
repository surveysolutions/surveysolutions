﻿using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_no_featured_questions_ : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire &&
                    repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionaire);

            interview = Create.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterviewCreatedOnClient(questionnaireId, questionnaireVersion, interviewStatus, featuredQuestionsMeta, isValid, userId);

        It should_event_context_contains_4_events = () =>
            eventContext.Events.Count().ShouldEqual(4);

        It should_raise_InterviewCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewOnClientCreated>();

        It should_provide_questionnaire_id_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewOnClientCreated>()
                .QuestionnaireId.ShouldEqual(questionnaireId);

        It should_provide_questionnaire_version_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewOnClientCreated>()
                .QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        It should_raise_SynchronizationMetadataApplied_event = () =>
            eventContext.ShouldContainEvent<SynchronizationMetadataApplied>();

        It should_provide_status_in_SynchronizationMetadataApplied_event = () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .Status.ShouldEqual(interviewStatus);

        It should_provide_userId_in_SynchronizationMetadataApplied_event = () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .UserId.ShouldEqual(userId);

        It should_provide_InterviewDeclaredValid_event = () =>
            eventContext.ShouldContainEvent<InterviewDeclaredValid>();

        It should_provide_InterviewStatusChanged_event = () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>();
        
        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static long questionnaireVersion = 18;
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static bool isValid = true;
        private static AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null;
        private static InterviewStatus interviewStatus = InterviewStatus.Completed;
        private static Interview interview;
    }
}