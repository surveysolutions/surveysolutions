using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerInterviewsControllerTests.v2
{
    [TestFixture]
    internal class when_posting_invalid_image_by_interview : InterviewsApiV2ControllerTestsContext
    {
        [Test]
        public void should_return_unsupported_media_type_and_not_store_image_when_format_is_unknown()
        {
            var imageStorage = new Mock<IImageFileStorage>();
            var imageProcessingService = new Mock<IImageProcessingService>();
            var brokenImageStorage = new Mock<IBrokenImageFileStorage>();
            imageProcessingService.Setup(x => x.Validate(It.IsAny<byte[]>())).Throws(new UnknownImageFormatException("Unsupported image format"));
            var controller = CreateInterviewerInterviewsController(imageFileStorage: imageStorage.Object, imageProcessingService: imageProcessingService.Object, brokenImageFileStorage: brokenImageStorage.Object);

            var result = controller.PostImage(CreateRequest());

            Assert.That(((StatusCodeResult)result).StatusCode, Is.EqualTo(StatusCodes.Status415UnsupportedMediaType));
            imageStorage.Verify(x => x.StoreInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
            brokenImageStorage.Verify(x => x.StoreInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_return_unsupported_media_type_and_not_store_image_when_content_is_invalid()
        {
            var imageStorage = new Mock<IImageFileStorage>();
            var imageProcessingService = new Mock<IImageProcessingService>();
            var brokenImageStorage = new Mock<IBrokenImageFileStorage>();
            imageProcessingService.Setup(x => x.Validate(It.IsAny<byte[]>())).Throws(new InvalidImageContentException("Invalid image content"));
            var controller = CreateInterviewerInterviewsController(imageFileStorage: imageStorage.Object, imageProcessingService: imageProcessingService.Object, brokenImageFileStorage: brokenImageStorage.Object);

            var result = controller.PostImage(CreateRequest());

            Assert.That(((StatusCodeResult)result).StatusCode, Is.EqualTo(StatusCodes.Status415UnsupportedMediaType));
            imageStorage.Verify(x => x.StoreInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
            brokenImageStorage.Verify(x => x.StoreInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
        }

        private static PostFileRequest CreateRequest() => new PostFileRequest
        {
            InterviewId = Guid.Parse("11111111111111111111111111111111"),
            FileName = "image.jpg",
            Data = Convert.ToBase64String(new byte[] { 1, 234, 21, 0, 54, 1, 66, 78 })
        };
    }
}
