using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class when_marking_interview_as_received : InterviewsApiV2ControllerTestsContext
    {
        private Establish context = () =>
        {
            controller = CreateInterviewerInterviewsController(
                commandService: mockOfCommandService.Object);
        };

        Because of = () => controller.LogInterviewAsSuccessfullyHandled(interviewId);

        It should_executed_command_to_mark_interview_as_received = () =>
            mockOfCommandService.Verify(x=>x.Execute(Moq.It.IsAny<MarkInterviewAsReceivedByInterviewer>(), Moq.It.IsAny<string>()), Times.Once);
        
        
        private static InterviewsApiV2Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
    }
}
