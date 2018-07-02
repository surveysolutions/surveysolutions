using System;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class when_posting_image_by_interview : InterviewsApiV2ControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateInterviewerInterviewsController(
                imageFileStorage: mockOflainInterviewFileStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() => controller.PostImage(new PostFileRequest { InterviewId = interviewId, FileName =  imageFileName, Data = imageAsBase64String });

        [NUnit.Framework.Test] public void should_store_image_to_plain_storage () =>
            mockOflainInterviewFileStorage.Verify(x=>x.StoreInterviewBinaryData(interviewId, imageFileName, imageBytes, null), Times.Once);
        
        
        private static InterviewsApiV2Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string imageFileName = "image.png";
        private static readonly byte[] imageBytes = {1, 234, 21, 0, 54, 1, 66, 78};
        private static readonly string imageAsBase64String = Convert.ToBase64String(imageBytes);
        private static readonly Mock<IImageFileStorage> mockOflainInterviewFileStorage = new Mock<IImageFileStorage>();
    }
}
