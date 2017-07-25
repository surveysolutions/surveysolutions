using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    internal class when_sync_package_does_not_contain_interview_key: InterviewPackagesServiceTestsContext
    {
        [Test]
        public void should_generate_interview_key_from_generator()
        {
            Mock<ICommandService> commandService = new Mock<ICommandService>();
            var service = CreateInterviewPackagesService(commandService: commandService.Object);

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage());

            // Assert
            commandService.Verify(x => x.Execute(It.Is<SynchronizeInterviewEventsCommand>(cmd => cmd.InterviewKey == GeneratedInterviewKey), It.IsAny<string>()));
        }
    }
}