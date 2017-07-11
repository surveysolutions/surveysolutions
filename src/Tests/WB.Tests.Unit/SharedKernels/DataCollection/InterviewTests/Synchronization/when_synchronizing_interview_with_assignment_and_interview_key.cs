using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_synchronizing_interview_with_assignment_interview_key : StatefulInterviewTestsContext
    {
        [Test]
        public void should_apply_interview_key()
        {
            var key = Create.Entity.InterviewKey(5);
            var assignmentId = 4;

            var syncDto = Create.Entity.InterviewSynchronizationDto(interviewKey: key, assignmentId: assignmentId);
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);
            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: false, questionnaireRepository: questionnaireRepository);
            var eventContext = new EventContext();

            // Act
            interview.Synchronize(Create.Command.Synchronize(Id.g2, syncDto));

            // Assert
            eventContext.ShouldContainEvent<InterviewKeyAssigned>(e => e.Key.Equals(key));
        }
    }
}