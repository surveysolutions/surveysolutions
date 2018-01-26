using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    internal class when_sync_package_does_not_contain_interview_key
    {
        [Test]
        public void should_generate_interview_key_from_generator()
        {
            SynchronizeInterviewEventsCommand syncCommand = null;
            Mock<ICommandService> commandService = new Mock<ICommandService>();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback((ICommand c, string o) => { syncCommand = c as SynchronizeInterviewEventsCommand; });

            var service = Create.Service.InterviewPackagesService(commandService: commandService.Object);

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage());

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.InterviewKey, Is.EqualTo(new InterviewKey(5533)));
        }
    }
}