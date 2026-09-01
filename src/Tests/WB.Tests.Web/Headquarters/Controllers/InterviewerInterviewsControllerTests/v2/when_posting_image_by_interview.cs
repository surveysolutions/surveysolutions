using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerInterviewsControllerTests.v2
{
    internal class when_posting_image_by_interview : InterviewsApiV2ControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            controller = CreateInterviewerInterviewsController(
                imageFileStorage: mockOflainInterviewFileStorage.Object,
                imageProcessingService: imageProcessingService.Object);
            result = BecauseOf();
        }

        public IActionResult BecauseOf() => controller.PostImage(new PostFileRequest
            {InterviewId = interviewId, FileName = imageFileName, Data = imageAsBase64String});

        [NUnit.Framework.Test]
        public void should_store_image_to_plain_storage() =>
            mockOflainInterviewFileStorage.Verify(
                x => x.StoreInterviewBinaryData(interviewId, imageFileName, imageBytes, null), Times.Once);

        [NUnit.Framework.Test]
        public void should_validate_image_once() =>
            imageProcessingService.Verify(x => x.Validate(imageBytes), Times.Once);

        [NUnit.Framework.Test]
        public void should_return_no_content() =>
            NUnit.Framework.Assert.That(((StatusCodeResult)result).StatusCode, NUnit.Framework.Is.EqualTo(StatusCodes.Status204NoContent));

        private static InterviewsApiV2Controller controller;
        private static IActionResult result;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string imageFileName = "image.png";
        private static readonly byte[] imageBytes = {1, 234, 21, 0, 54, 1, 66, 78};
        private static readonly string imageAsBase64String = Convert.ToBase64String(imageBytes);
        private static readonly Mock<IImageFileStorage> mockOflainInterviewFileStorage = new Mock<IImageFileStorage>();
        private static readonly Mock<WB.UI.Shared.Web.Services.IImageProcessingService> imageProcessingService = new Mock<WB.UI.Shared.Web.Services.IImageProcessingService>();
    }
}
