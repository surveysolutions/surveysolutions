using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.SupervisorInterviewsControllerTests.v1;
using WB.UI.Headquarters.API.DataCollection.Supervisor.v1;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.SupervisorInterviewsControllerTests.v1
{
    [TestFixture]
    internal class InterviewsApiV1ControllerTests : InterviewsApiV1ControllerTestsContext
    {
        [Test]
        public void when_marking_interview_as_received_by_supervisor_should_executed_command_to_mark_interview_as_received()
        {
            Guid interviewId = Guid.Parse("11111111111111111111111111111111");
            Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
            var controller = CreateSupervisorInterviewsController(commandService: mockOfCommandService.Object);

            controller.LogInterviewAsSuccessfullyHandled(interviewId);

            mockOfCommandService.Verify(x => x.Execute(Moq.It.IsAny<MarkInterviewAsReceivedBySupervisor>(), Moq.It.IsAny<string>()), Times.Once);
        }
    }
}
