using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v1
{
    internal class when_posting_sync_package : InterviewsApiV1ControllerTestsContext
    {
        private Establish context = () =>
        {
            controller = CreateInterviewerInterviewsController(
                incomingSyncPackagesQueue: mockOfIncomingSyncPackagesQueue.Object);
        };

        Because of = () => controller.Post(interviewId, package);

        It should_add_sync_package_to_sync_packages_queue = () =>
            mockOfIncomingSyncPackagesQueue.Verify(x=>x.Enqueue(interviewId, package), Times.Once);
        
        
        private static InterviewsApiV1Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string package = "package content";
        private static readonly Mock<IIncomingSyncPackagesQueue> mockOfIncomingSyncPackagesQueue = new Mock<IIncomingSyncPackagesQueue>();
    }
}
