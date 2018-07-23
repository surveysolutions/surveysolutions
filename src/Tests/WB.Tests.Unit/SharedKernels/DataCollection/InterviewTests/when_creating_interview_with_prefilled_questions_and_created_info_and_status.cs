using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_prefilled_questions_and_created_info_and_status : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Stub<IQuestionnaireStorage>.Returning(questionaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.CreateInterviewFromSynchronizationMetadata(interviewId, userId, questionnaireId, 1, interviewStatus, 
                featuredQuestionsMeta, comments, rejectedDateTime, 
                interviewerAssignedDateTime, isValid, createdOnClient, DateTimeOffset.Now);

        [NUnit.Framework.Test] public void should_event_context_contains_3_events () =>
            eventContext.Events.Count().Should().Be(3);

        [NUnit.Framework.Test] public void should_raise_SynchronizationMetadataApplied_event () =>
            eventContext.ShouldContainEvent<SynchronizationMetadataApplied>();

        [NUnit.Framework.Test] public void should_provide_created_onclient_in_SynchronizationMetadataApplied_event () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .CreatedOnClient.Should().Be(createdOnClient);
        
        [NUnit.Framework.Test] public void should_provide_status_in_SynchronizationMetadataApplied_event () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .Status.Should().Be(interviewStatus);

        [NUnit.Framework.Test] public void should_provide_status_in_InterviewStatusChanged_event () =>
            eventContext.GetEvent<InterviewStatusChanged>()
                .Status.Should().Be(interviewStatus);
        
        [NUnit.Framework.Test] public void should_raise_InterviewDeclaredValid_event () =>
            eventContext.ShouldContainEvent<InterviewDeclaredValid>();
        
        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static long questionnaireVersion = 18;
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid interviewId = Guid.Parse("11000000000000000000000000000000");
        private static string comments = "status chance comment";
        private static DateTime? rejectedDateTime = DateTime.Now;
        private static DateTime? interviewerAssignedDateTime = DateTime.Now;
        private static AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null;
        private static InterviewStatus interviewStatus = InterviewStatus.Completed;

        private static bool isValid = true;
        private static bool createdOnClient = true;
        private static Interview interview;
    }
}
