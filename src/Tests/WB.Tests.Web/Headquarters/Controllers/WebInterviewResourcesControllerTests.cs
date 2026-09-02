using FluentAssertions;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Controllers.Api.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Web.Headquarters.Controllers
{
    [TestFixture]
    [TestOf(typeof(WebInterviewResourcesController))]
    internal class WebInterviewResourcesControllerTests
    {
        [Test]
        public void when_thumbnail_cannot_be_created_should_return_original_attachment_content()
        {
            var attachment = new AttachmentContent
            {
                Content = new byte[] { 1, 234, 21, 0, 54, 1, 66, 78 },
                ContentType = "image/heic",
                ContentHash = contentId
            };

            var attachmentStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
            attachmentStorage.Setup(x => x.GetById(contentId)).Returns(attachment);

            var imageProcessingService = new Mock<IImageProcessingService>();
            imageProcessingService
                .Setup(x => x.ResizeImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new UnknownImageFormatException("Unsupported image format"));

            var controller = CreateController(imageProcessingService.Object, attachmentStorage.Object);

            var result = controller.Content(interviewId, contentId) as FileContentResult;

            result.Should().NotBeNull();
            result.FileContents.Should().BeEquivalentTo(attachment.Content);
            result.ContentType.Should().Be(attachment.ContentType);
        }

        [Test]
        public async Task when_image_thumbnail_cannot_be_created_should_return_binary_download()
        {
            var imageFileStorage = new Mock<IImageFileStorage>();
            imageFileStorage
                .Setup(x => x.GetInterviewBinaryDataAsync(interviewGuid, imageFileName))
                .ReturnsAsync(imageContent);

            var imageProcessingService = new Mock<IImageProcessingService>();
            imageProcessingService
                .Setup(x => x.ResizeImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new UnknownImageFormatException("Unsupported image format"));

            var controller = CreateController(
                imageProcessingService.Object,
                Mock.Of<IPlainStorageAccessor<AttachmentContent>>(),
                imageFileStorage.Object,
                new InterviewTreeMultimediaQuestion(imageFileName, null));

            var result = await controller.Image(interviewId, questionId) as FileContentResult;

            result.Should().NotBeNull();
            result!.FileContents.Should().BeEquivalentTo(imageContent);
            result.ContentType.Should().Be("application/octet-stream");
            result.FileDownloadName.Should().Be(imageFileName);
            controller.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        }

        [Test]
        public async Task when_full_size_unsupported_image_is_requested_should_return_binary_download()
        {
            var imageFileStorage = new Mock<IImageFileStorage>();
            imageFileStorage
                .Setup(x => x.GetInterviewBinaryDataAsync(interviewGuid, imageFileName))
                .ReturnsAsync(imageContent);

            var imageProcessingService = new Mock<IImageProcessingService>();
            imageProcessingService
                .Setup(x => x.ResizeImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new UnknownImageFormatException("Unsupported image format"));

            var controller = CreateController(
                imageProcessingService.Object,
                Mock.Of<IPlainStorageAccessor<AttachmentContent>>(),
                imageFileStorage.Object,
                new InterviewTreeMultimediaQuestion(imageFileName, null));
            controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?fullSize=1");

            var result = await controller.Image(interviewId, questionId) as FileContentResult;

            result.Should().NotBeNull();
            result!.FileContents.Should().BeEquivalentTo(imageContent);
            result.ContentType.Should().Be("application/octet-stream");
            result.FileDownloadName.Should().Be(imageFileName);
            controller.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        }

        private static WebInterviewResourcesController CreateController(
            IImageProcessingService imageProcessingService,
            IPlainStorageAccessor<AttachmentContent> attachmentStorage,
            IImageFileStorage imageFileStorage = null,
            InterviewTreeMultimediaQuestion multimediaQuestion = null)
        {
            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
            interview.Setup(x => x.Id).Returns(interviewGuid);
            interview.Setup(x => x.GetMultimediaQuestion(It.IsAny<WB.Core.SharedKernels.DataCollection.Identity>()))
                .Returns(multimediaQuestion);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(interviewId)).Returns(interview.Object);

            var controller = new WebInterviewResourcesController(
                Mock.Of<IAuthorizedUser>(),
                imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                interviewRepository.Object,
                imageProcessingService,
                attachmentStorage,
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<ILogger<WebInterviewResourcesController>>());

            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            return controller;
        }

        private const string interviewId = "11111111111111111111111111111111";
        private const string contentId = "content-id";
        private const string questionId = "111111111111111111111111111111111";
        private const string imageFileName = "image.heic";
        private static readonly Guid interviewGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly byte[] imageContent = { 1, 234, 21, 0, 54, 1, 66, 78 };
    }
}
