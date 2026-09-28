using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.SurveySolutions.Documents;
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
        public void when_requesting_image_and_format_is_not_supported_should_return_original_bytes()
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

        [Test]
        public void when_requesting_attachment_and_format_is_not_supported_should_return_original_bytes()
        {
            var attachmentStorage = new Mock<ICacheStorage<QuestionnaireAttachment, string>>();
            attachmentStorage
                .Setup(x => x.Get(contentId, interviewId))
                .Returns(new QuestionnaireAttachment(Guid.NewGuid(), new AttachmentContent
                {
                    Content = fileContent,
                    ContentType = mimeType
                }));

            var imageProcessingService = new Mock<IImageProcessingService>();
            imageProcessingService
                .Setup(x => x.ResizeImage(fileContent, 200, 1920))
                .Throws(new UnknownImageFormatException("Unsupported image format"));

            var controller = new WebInterviewResourcesController(
                attachmentStorage.Object,
                imageProcessingService.Object,
                Mock.Of<ICacheStorage<MultimediaFile, string>>(),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IQuestionnaireStorage>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var result = controller.GetContent(interviewIdString, contentId) as FileContentResult;

            result.Should().NotBeNull();
            result!.FileContents.Should().BeEquivalentTo(fileContent);
            result.ContentType.Should().Be(mimeType);
        }

        [Test]
        public void when_requesting_named_attachment_and_format_is_not_supported_should_return_original_bytes()
        {
            var attachmentId = Guid.NewGuid();
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
            var attachment = new Attachment
            {
                AttachmentId = attachmentId,
                ContentId = contentId,
                Name = attachmentName
            };

            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.Id).Returns(interviewId);
            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireIdentity);
            interview.Setup(x => x.Language).Returns((string)null);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(interviewIdString)).Returns(interview.Object);

            var questionnaire = new Mock<IQuestionnaire>();
            questionnaire.Setup(x => x.GetAttachmentIdByName(attachmentName)).Returns(attachmentId);
            questionnaire.Setup(x => x.GetAttachmentById(attachmentId)).Returns(attachment);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            questionnaireStorage
                .Setup(x => x.GetQuestionnaireOrThrow(questionnaireIdentity, null))
                .Returns(questionnaire.Object);

            var attachmentStorage = new Mock<ICacheStorage<QuestionnaireAttachment, string>>();
            attachmentStorage
                .Setup(x => x.Get(contentId, interviewId))
                .Returns(new QuestionnaireAttachment(attachmentId, new AttachmentContent
                {
                    Content = fileContent,
                    ContentType = mimeType
                }));

            var imageProcessingService = new Mock<IImageProcessingService>();
            imageProcessingService
                .Setup(x => x.ResizeImage(fileContent, 100, 1920))
                .Throws(new UnknownImageFormatException("Unsupported image format"));

            var controller = new WebInterviewResourcesController(
                attachmentStorage.Object,
                imageProcessingService.Object,
                Mock.Of<ICacheStorage<MultimediaFile, string>>(),
                interviewRepository.Object,
                questionnaireStorage.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var result = controller.GetAttachment(interviewIdString, attachmentName) as FileContentResult;

            result.Should().NotBeNull();
            result!.FileContents.Should().BeEquivalentTo(fileContent);
            result.ContentType.Should().Be(mimeType);
        }

        private const string interviewIdString = "11111111111111111111111111111111";
        private static readonly Guid interviewId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const string questionId = "22222222222222222222222222222222";
        private const string contentId = "content-id";
        private const string attachmentName = "photo";
        private const string fileName = "image.heic";
        private const string mimeType = "image/heic";
        private static readonly byte[] fileContent = { 1, 234, 21, 0, 54, 1, 66, 78 };
    }
}
