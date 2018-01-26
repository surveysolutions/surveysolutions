using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    class when_sync_package_with_new_interview
    {
        private static Mock<ICommandService> commandService;
        private static SynchronizeInterviewEventsCommand syncCommand = null;

        [SetUp]
        public void Setup()
        {
            commandService = new Mock<ICommandService>();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback((ICommand c, string o) => { syncCommand = c as SynchronizeInterviewEventsCommand; });
        }

        [Test]
        public void When_interviewer_was_moved_to_another_team_after_interview_was_created()
        {
            var service = Create.Service.InterviewPackagesService(commandService: commandService.Object);

            var oldSupervisorId = Id.gE;
            var newSupervisorId = Id.gB;

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage(Id.g1, Create.Event.SupervisorAssigned(Id.gC, oldSupervisorId)));

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.NewSupervisorId, Is.EqualTo(newSupervisorId));
        }

        [Test]
        public void When_interviewer_was_not_moved_to_another_team_after_interview_was_created()
        {
            var service = Create.Service.InterviewPackagesService(commandService: commandService.Object);

            var oldSupervisorId = Id.gB;

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage(Id.g1, Create.Event.SupervisorAssigned(Id.gC, oldSupervisorId)));

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.NewSupervisorId, Is.Null);
        }
    }
}