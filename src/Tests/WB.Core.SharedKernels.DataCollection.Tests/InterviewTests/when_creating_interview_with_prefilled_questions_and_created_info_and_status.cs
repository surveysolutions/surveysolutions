using System;
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

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_creating_interview_with_prefilled_questions_and_created_info_and_status : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire &&
                    repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, interviewStatus, featuredQuestionsMeta, comments, isValid, createdOnClient);

        It should_event_context_contains_3_events = () =>
            eventContext.Events.Count().ShouldEqual(3);

        It should_raise_SynchronizationMetadataApplied_event = () =>
            eventContext.ShouldContainEvent<SynchronizationMetadataApplied>();

        It should_provide_created_onclient_in_SynchronizationMetadataApplied_event = () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .CreatedOnClient.ShouldEqual(createdOnClient);
        
        It should_provide_status_in_SynchronizationMetadataApplied_event = () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .Status.ShouldEqual(interviewStatus);

        It should_provide_status_in_InterviewStatusChanged_event = () =>
            eventContext.GetEvent<InterviewStatusChanged>()
                .Status.ShouldEqual(interviewStatus);
        
        It should_raise_InterviewDeclaredValid_event = () =>
            eventContext.ShouldContainEvent<InterviewDeclaredValid>();
        
        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static long questionnaireVersion = 18;
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid interviewId = Guid.Parse("11000000000000000000000000000000");
        private static string comments = "status chance comment";
        private static AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null;
        private static InterviewStatus interviewStatus = InterviewStatus.Completed;

        private static bool isValid = true;
        private static bool createdOnClient = true;
    }
}