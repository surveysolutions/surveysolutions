using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v1
{
    internal class when_posting_image_by_interview : InterviewsApiV1ControllerTestsContext
    {
        private Establish context = () =>
        {
            controller = CreateInterviewerInterviewsController(
                plainInterviewFileStorage: mockOflainInterviewFileStorage.Object);
        };

        Because of = () => controller.PostImage(new PostFileRequest { InterviewId = interviewId, FileName =  imageFileName, Data = imageAsBase64String });

        It should_store_image_to_plain_storage = () =>
            mockOflainInterviewFileStorage.Verify(x=>x.StoreInterviewBinaryData(interviewId, imageFileName, imageBytes), Times.Once);
        
        
        private static InterviewsApiV1Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string imageFileName = "image.png";
        private static readonly byte[] imageBytes = {1, 234, 21, 0, 54, 1, 66, 78};
        private static readonly string imageAsBase64String = Convert.ToBase64String(imageBytes);
        private static readonly Mock<IPlainInterviewFileStorage> mockOflainInterviewFileStorage = new Mock<IPlainInterviewFileStorage>();
    }
}
