using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    internal class when_sync_package_contains_unique_interview_key
    {
        [Test]
        public void should_not_generate_new_interview_key()
        {
            SynchronizeInterviewEventsCommand syncCommand = null;
            Mock<ICommandService> commandService = new Mock<ICommandService>();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback((ICommand c, string o) => { syncCommand = c as SynchronizeInterviewEventsCommand; });

            var service = Create.Service.InterviewPackagesService(commandService: commandService.Object);

            InterviewKeyAssigned keyAssignedEvent = Create.Event.InterviewKeyAssigned();
            var aggregateRootEvent = Create.Event.AggregateRootEvent(keyAssignedEvent);

            var interviewPackage = Create.Entity.InterviewPackage(events: new[]{aggregateRootEvent});
            // Act
            service.ProcessPackage(interviewPackage);

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.InterviewKey, Is.Null);
        }
    }
}