using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestFixture]
    [TestOf(typeof(WebInterviewBinaryController))]
    public class WebInterviewBinaryControllerTests
    {
        private static readonly Guid QuestionId = Id.g2;
        private static readonly Guid UserId = Id.gA;

        [Test]
        public async Task Image_when_no_previous_answer_stores_new_file_and_executes_command()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            var imageFileStorage = new Mock<IImageFileStorage>();
            var commandService = new Mock<ICommandService>();
            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("photo.jpg", "image/jpeg");

            var result = await controller.Image(interview.Id, questionIdentity.ToString(), file);

            Assert.That(result, Is.InstanceOf<JsonResult>());
            var expectedFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg");
            imageFileStorage.Verify(s => s.StoreInterviewBinaryData(interview.Id, expectedFilename, It.IsAny<byte[]>(), "image/jpeg"), Times.Once);
            commandService.Verify(s => s.Execute(It.Is<AnswerPictureQuestionCommand>(c => c.PictureFileName == expectedFilename), null), Times.Once);
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Image_when_previous_answer_has_same_extension_does_not_remove_old_file()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            var existingFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg");
            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, existingFilename);

            var imageFileStorage = new Mock<IImageFileStorage>();
            var commandService = new Mock<ICommandService>();
            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("newphoto.jpg", "image/jpeg");

            await controller.Image(interview.Id, questionIdentity.ToString(), file);

            // same filename produced — no removal needed
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Image_when_previous_answer_differs_only_by_extension_case_does_not_remove_old_file()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, "photo__.JPG");

            var imageFileStorage = new Mock<IImageFileStorage>();
            var commandService = new Mock<ICommandService>();
            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("newphoto.jpg", "image/jpeg");

            await controller.Image(interview.Id, questionIdentity.ToString(), file);

            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Image_when_previous_answer_has_different_extension_removes_old_file()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            var existingFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg");
            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, existingFilename);

            var imageFileStorage = new Mock<IImageFileStorage>();
            var commandService = new Mock<ICommandService>();
            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("photo.png", "image/png");

            await controller.Image(interview.Id, questionIdentity.ToString(), file);

            var newFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".png");
            imageFileStorage.Verify(s => s.StoreInterviewBinaryData(interview.Id, newFilename, It.IsAny<byte[]>(), "image/png"), Times.Once);
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(interview.Id, existingFilename), Times.Once);
        }

        [Test]
        public async Task Image_when_command_throws_removes_newly_stored_file()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            var imageFileStorage = new Mock<IImageFileStorage>();
            var commandService = new Mock<ICommandService>();
            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), null))
                .Throws(new Exception("command failure"));

            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("photo.jpg", "image/jpeg");

            Assert.ThrowsAsync<Exception>(() => controller.Image(interview.Id, questionIdentity.ToString(), file));

            var filename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg");
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(interview.Id, filename), Times.Once);
        }

        [Test]
        public async Task Image_when_old_file_removal_throws_does_not_delete_new_file()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            var existingFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg");
            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, existingFilename);

            var imageFileStorage = new Mock<IImageFileStorage>();
            imageFileStorage
                .Setup(s => s.RemoveInterviewBinaryData(It.IsAny<Guid>(), existingFilename))
                .ThrowsAsync(new Exception("storage failure"));

            var commandService = new Mock<ICommandService>();
            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("photo.png", "image/png");

            // should not throw — old-file removal is best-effort
            var result = await controller.Image(interview.Id, questionIdentity.ToString(), file);

            Assert.That(result, Is.InstanceOf<JsonResult>());
            var newFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".png");
            // new file must NOT be removed
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(interview.Id, newFilename), Times.Never);
        }

        [Test]
        public async Task Image_after_answer_was_cleared_stores_new_file_without_removing_previous()
        {
            // Covers the clear-then-upload flow: RemoveAnswer was called first (which deleted
            // the old storage entry), so IsAnswered() is false when Image() runs.
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);

            // Simulate state after RemoveAnswer: answer the question then clear it so IsAnswered() == false.
            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow,
                AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg"));
            interview.RemoveAnswer(QuestionId, RosterVector.Empty, UserId, DateTimeOffset.UtcNow);

            var imageFileStorage = new Mock<IImageFileStorage>();
            var commandService = new Mock<ICommandService>();
            var controller = CreateController(interview, imageFileStorage.Object, commandService.Object);

            var questionIdentity = Identity.Create(QuestionId, RosterVector.Empty);
            var file = CreateFormFile("photo.jpg", "image/jpeg");

            var result = await controller.Image(interview.Id, questionIdentity.ToString(), file);

            Assert.That(result, Is.InstanceOf<JsonResult>());
            var newFilename = AnswerUtils.GetPictureFileName("photo", RosterVector.Empty, ".jpg");
            imageFileStorage.Verify(s => s.StoreInterviewBinaryData(interview.Id, newFilename, It.IsAny<byte[]>(), "image/jpeg"), Times.Once);
            commandService.Verify(s => s.Execute(It.Is<AnswerPictureQuestionCommand>(c => c.PictureFileName == newFilename), null), Times.Once);
            // RemoveAnswer already cleaned up the old file; Image() must not attempt a second removal.
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }


        private static WebInterviewBinaryController CreateController(
            StatefulInterview interview,
            IImageFileStorage imageFileStorage = null,
            ICommandService commandService = null)
        {
            var repo = Mock.Of<IStatefulInterviewRepository>(r =>
                r.Get(interview.Id.FormatGuid()) == interview);

            var controller = new WebInterviewBinaryController(
                statefulInterviewRepository: repo,
                commandService: commandService ?? Mock.Of<ICommandService>(),
                imageProcessingService: Mock.Of<IImageProcessingService>(),
                webInterviewNotificationService: Mock.Of<IWebInterviewNotificationService>(),
                audioFileStorage: Mock.Of<IAudioFileStorage>(),
                audioProcessingService: Mock.Of<IAudioProcessingService>(),
                imageFileStorage: imageFileStorage ?? Mock.Of<IImageFileStorage>());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static IFormFile CreateFormFile(string fileName, string contentType)
        {
            var content = Encoding.UTF8.GetBytes("fake-image-bytes");
            var stream = new MemoryStream(content);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.ContentType).Returns(contentType);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((s, _) =>
                {
                    stream.Position = 0;
                    stream.CopyTo(s);
                })
                .Returns(Task.CompletedTask);
            return file.Object;
        }
    }
}
