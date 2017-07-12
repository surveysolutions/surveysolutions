using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    internal class when_sync_package_contains_unique_interview_key : InterviewPackagesServiceTestsContext
    {
        [Test]
        public void should_not_generate_new_interview_key()
        {
            Mock<ICommandService> commandService = new Mock<ICommandService>();
            var service = CreateInterviewPackagesService(commandService: commandService.Object);

            InterviewKeyAssigned keyAssignedEvent = Create.Event.InterviewKeyAssigned();
            var aggregateRootEvent = Create.Event.AggregateRootEvent(keyAssignedEvent);

            var interviewPackage = Create.Entity.InterviewPackage(events: new[]{aggregateRootEvent});
            // Act
            service.ProcessPackage(interviewPackage);

            // Assert
            commandService.Verify(x => x.Execute(It.Is<SynchronizeInterviewEventsCommand>(cmd => cmd.InterviewKey == null), It.IsAny<string>()));
        }
    }
}