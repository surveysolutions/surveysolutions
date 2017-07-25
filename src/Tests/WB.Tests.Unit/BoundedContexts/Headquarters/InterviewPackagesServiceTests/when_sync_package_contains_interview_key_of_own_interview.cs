using System;
using Main.Core.Events;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    class when_sync_package_contains_interview_key_of_own_interview: InterviewPackagesServiceTestsContext
    {
        [Test]
        public void should__NOT__generate_new_interview_key()
        {
            InterviewKey existingInterviewKey = new InterviewKey(1324);
            Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            Mock<ICommandService> commandService = new Mock<ICommandService>();

            var interviews = new TestInMemoryWriter<InterviewSummary>();
            var existingSummary = Create.Entity.InterviewSummary(interviewId: interviewId, key: existingInterviewKey.ToString());
            interviews.Store(existingSummary, "id");

            InterviewKeyAssigned keyAssignedEventZero = Create.Event.InterviewKeyAssigned(new InterviewKey(1152998));
            InterviewKeyAssigned keyAssignedEvent = Create.Event.InterviewKeyAssigned(existingInterviewKey);
            var aggregateRootEventZero = Create.Event.AggregateRootEvent(keyAssignedEventZero);
            var aggregateRootEvent = Create.Event.AggregateRootEvent(keyAssignedEvent);

            var service = CreateInterviewPackagesService(interviews: interviews, commandService: commandService.Object);

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage(interviewId, events: new[] { aggregateRootEventZero, aggregateRootEvent }));

            // Assert
            commandService.Verify(x => x.Execute(It.Is<SynchronizeInterviewEventsCommand>(cmd => cmd.InterviewKey == null), It.IsAny<string>()));
        }
    }
}