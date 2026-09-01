using FluentAssertions;
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

        private static WebInterviewResourcesController CreateController(
            IImageProcessingService imageProcessingService,
            IPlainStorageAccessor<AttachmentContent> attachmentStorage)
        {
            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(interviewId)).Returns(interview.Object);

            var controller = new WebInterviewResourcesController(
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IImageFileStorage>(),
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
    }
}
