using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerInterviewsControllerTests.v2
{
    internal class when_marking_interview_as_received : InterviewsApiV2ControllerTestsContext
    {
        [Test]
        public void should_executed_command_to_mark_interview_as_received()
        {
            Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
            
            var controller = CreateInterviewerInterviewsController(
                commandService: mockOfCommandService.Object);
            // act
            controller.LogInterviewAsSuccessfullyHandled(Id.g1);
            
            // assert
            mockOfCommandService.Verify(
                x => x.Execute(It.IsAny<MarkInterviewAsReceivedByInterviewer>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
