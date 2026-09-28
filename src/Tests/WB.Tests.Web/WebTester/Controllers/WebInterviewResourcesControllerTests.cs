using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Controllers;
using WB.UI.WebTester.Services;

namespace WB.Tests.Web.WebTester.Controllers
{
    [TestFixture]
    [TestOf(typeof(WebInterviewResourcesController))]
    internal class WebInterviewResourcesControllerTests
    {
        [Test]
        public void when_image_resize_is_not_supported_should_return_original_stream()
        {
            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.Id).Returns(interviewId);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(interviewIdString)).Returns(interview.Object);

            var multimediaFile = new MultimediaFile(fileName, fileContent, null, mimeType);
            var mediaStorage = new Mock<ICacheStorage<MultimediaFile, string>>();
            mediaStorage.Setup(x => x.Get(fileName, interviewId)).Returns(multimediaFile);

            var imageProcessingService = new Mock<IImageProcessingService>();
            imageProcessingService
                .Setup(x => x.ResizeImage(fileContent, 200, 1920))
                .Throws(new UnknownImageFormatException("Unsupported image format"));

            var controller = new WebInterviewResourcesController(
                Mock.Of<ICacheStorage<QuestionnaireAttachment, string>>(),
                imageProcessingService.Object,
                mediaStorage.Object,
                interviewRepository.Object,
                Mock.Of<IQuestionnaireStorage>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var result = controller.Image(interviewIdString, questionId, fileName) as FileContentResult;

            result.Should().NotBeNull();
            result!.FileContents.Should().BeEquivalentTo(fileContent);
            result.ContentType.Should().Be(mimeType);
        }

        private const string interviewIdString = "11111111111111111111111111111111";
        private static readonly Guid interviewId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const string questionId = "22222222222222222222222222222222";
        private const string fileName = "image.heic";
        private const string mimeType = "image/heic";
        private static readonly byte[] fileContent = { 1, 234, 21, 0, 54, 1, 66, 78 };
    }
}
