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
    class when_sync_package_contains_interview_key_of_another_existing_interview
    {
        [Test]
        public void should_generate_new_interview_key()
        {
            InterviewKey existingInterviewKey = new InterviewKey(1324);
            
            SynchronizeInterviewEventsCommand syncCommand = null;
            Mock<ICommandService> commandService = new Mock<ICommandService>();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback((ICommand c, string o) => { syncCommand = c as SynchronizeInterviewEventsCommand; });

            var interviews = new TestInMemoryWriter<InterviewSummary>();
            var existingSummary = Create.Entity.InterviewSummary(key: existingInterviewKey.ToString());
            interviews.Store(existingSummary, "id");

            InterviewKeyAssigned keyAssignedEvent = Create.Event.InterviewKeyAssigned(existingInterviewKey);
            var aggregateRootEvent = Create.Event.AggregateRootEvent(keyAssignedEvent);

            var service = Create.Service.InterviewPackagesService(interviews: interviews, commandService: commandService.Object);

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage(events: new[] { aggregateRootEvent }));

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.InterviewKey, Is.EqualTo(new InterviewKey(5533)));
        }
    }
}