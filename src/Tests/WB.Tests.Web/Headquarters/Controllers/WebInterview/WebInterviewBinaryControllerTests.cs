using System;
using System.IO;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Web.Headquarters.Controllers.WebInterview;

[TestFixture]
[TestOf(typeof(WebInterviewBinaryController))]
public class WebInterviewBinaryControllerTests
{
    [Test]
    public async Task when_uploading_new_picture_with_different_extension_should_remove_old_picture_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.jpg";

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "photo",
            QuestionType.Multimedia,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, null));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            Mock.Of<ICommandService>(),
            Mock.Of<IWebInterviewNotificationService>(),
            CreateBinaryServices(imageFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        await controller.Image(interviewId, questionIdentity.ToString(), formFile);

        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, "photo__.png", It.IsAny<byte[]>(), "image/png"), Times.Once);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, oldFileName), Times.Once);
    }

    [Test]
    public async Task when_uploading_new_picture_with_same_filename_should_not_remove_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.PNG";

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "photo",
            QuestionType.Multimedia,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, null));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        imageFileStorage.Setup(x => x.IsEquivalentFileName(oldFileName, "photo__.png")).Returns(true);
        imageFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, oldFileName))
            .ReturnsAsync(new byte[] { 7, 8, 9 });
        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            Mock.Of<ICommandService>(),
            Mock.Of<IWebInterviewNotificationService>(),
            CreateBinaryServices(imageFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        await controller.Image(interviewId, questionIdentity.ToString(), formFile);

        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, "photo__.png", It.IsAny<byte[]>(), "image/png"), Times.Once);
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, It.IsAny<string>(), It.IsAny<byte[]>(), "image/png"), Times.Once);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, "photo__.png"), Times.Never);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, oldFileName), Times.Never);
    }

    [Test]
    public void when_uploading_picture_with_case_only_extension_change_and_command_fails_should_restore_original_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.PNG";
        var newFileName = "photo__.png";
        var oldData = new byte[] { 7, 8, 9 };
        var oldFileContentType = ContentTypeHelper.GetImageContentType(oldFileName);

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "photo",
            QuestionType.Multimedia,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, null));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        imageFileStorage.Setup(x => x.IsEquivalentFileName(oldFileName, newFileName)).Returns(true);
        imageFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, oldFileName)).ReturnsAsync(oldData);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>())).Throws(new InvalidOperationException("boom"));

        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            commandService.Object,
            Mock.Of<IWebInterviewNotificationService>(),
            CreateBinaryServices(imageFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Image(interviewId, questionIdentity.ToString(), formFile));
        Assert.That(exception, Is.Not.Null);

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, newFileName), Times.Once);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, oldFileName), Times.Never);
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, oldFileName, oldData, oldFileContentType), Times.Once);
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public void when_uploading_picture_with_case_only_extension_change_and_missing_old_file_and_command_fails_should_remove_new_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.PNG";
        var newFileName = "photo__.png";

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "photo",
            QuestionType.Multimedia,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, null));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        imageFileStorage.Setup(x => x.IsEquivalentFileName(oldFileName, newFileName)).Returns(true);
        imageFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, oldFileName)).ReturnsAsync((byte[])null);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>())).Throws(new InvalidOperationException("boom"));

        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            commandService.Object,
            Mock.Of<IWebInterviewNotificationService>(),
            CreateBinaryServices(imageFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Image(interviewId, questionIdentity.ToString(), formFile));
        Assert.That(exception, Is.Not.Null);

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, newFileName), Times.Once);
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, oldFileName, It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void when_uploading_picture_with_case_only_extension_change_and_store_fails_after_missing_old_file_should_remove_new_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.PNG";
        var newFileName = "photo__.png";

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "photo",
            QuestionType.Multimedia,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, null));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        imageFileStorage.Setup(x => x.IsEquivalentFileName(oldFileName, newFileName)).Returns(true);
        imageFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, oldFileName)).ReturnsAsync((byte[])null);
        imageFileStorage.Setup(x => x.StoreInterviewBinaryData(interviewId, newFileName, It.IsAny<byte[]>(), "image/png"))
            .Throws(new InvalidOperationException("boom"));

        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            Mock.Of<ICommandService>(),
            Mock.Of<IWebInterviewNotificationService>(),
            CreateBinaryServices(imageFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Image(interviewId, questionIdentity.ToString(), formFile));
        Assert.That(exception, Is.Not.Null);

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, newFileName), Times.Once);
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, oldFileName, It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void when_uploading_picture_and_post_commit_exception_occurs_should_not_rollback_committed_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "photo__.png";
        var previousData = new byte[] { 9, 8, 7 };
        DateTime? committedAnswerTimeUtc = null;

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "photo",
            QuestionType.Multimedia,
            null,
            null,
            null,
            false,
            false,
            false);

        var beforeUploadInterview = new Mock<IStatefulInterview>();
        beforeUploadInterview.SetupGet(x => x.Id).Returns(interviewId);
        beforeUploadInterview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        beforeUploadInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        beforeUploadInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(fileName, DateTime.UtcNow.AddMinutes(-1)));

        var afterCommitInterview = new Mock<IStatefulInterview>();
        afterCommitInterview.SetupGet(x => x.Id).Returns(interviewId);
        afterCommitInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        afterCommitInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(() => new InterviewTreeMultimediaQuestion(fileName, committedAnswerTimeUtc));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.SetupSequence(x => x.Get(It.IsAny<string>()))
            .Returns(beforeUploadInterview.Object)
            .Returns(beforeUploadInterview.Object)
            .Returns(afterCommitInterview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        imageFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, fileName)).ReturnsAsync(previousData);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Callback<ICommand, string>((command, _) => committedAnswerTimeUtc = ((AnswerPictureQuestionCommand)command).OriginDate.UtcDateTime)
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            commandService.Object,
            webInterviewNotificationService.Object,
            CreateBinaryServices(imageFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Image(interviewId, questionIdentity.ToString(), formFile));
        Assert.That(exception, Is.Not.Null);

        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, It.IsAny<byte[]>(), "image/png"), Times.Once);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Never);
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, previousData, ContentTypeHelper.GetImageContentType(fileName)), Times.Never);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Never);
    }

    [Test]
    public void when_uploading_audio_and_command_fails_should_restore_previous_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "audio__.aac";
        var previousData = new byte[] { 9, 8, 7 };

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "audio",
            QuestionType.Audio,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(2)));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var audioFileStorage = new Mock<IAudioFileStorage>();
        audioFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, fileName)).ReturnsAsync(previousData);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>())).Throws(new InterviewException("boom"));

        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            commandService.Object,
            Mock.Of<IWebInterviewNotificationService>(),
            CreateBinaryServices(Mock.Of<IImageFileStorage>(), audioFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "audio.aac")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/aac"
        };

        var exception = Assert.ThrowsAsync<InterviewException>(async () => await controller.Audio(interviewId, questionIdentity.ToString(), "1", formFile));
        Assert.That(exception, Is.Not.Null);

        audioFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Once);
        audioFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, previousData, ContentTypeHelper.GetAudioContentType(fileName)), Times.Once);
        statefulInterviewRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public void when_uploading_audio_and_store_throws_for_existing_file_should_restore_previous_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "audio__.aac";
        var previousData = new byte[] { 9, 8, 7 };

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "audio",
            QuestionType.Audio,
            null,
            null,
            null,
            false,
            false,
            false);

        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.Id).Returns(interviewId);
        interview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        interview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        interview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(2)));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(interview.Object);

        var audioFileStorage = new Mock<IAudioFileStorage>();
        audioFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, fileName)).ReturnsAsync(previousData);
        audioFileStorage.SetupSequence(x => x.StoreInterviewBinaryData(interviewId, fileName, It.IsAny<byte[]>(), "audio/aac"))
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            Mock.Of<ICommandService>(),
            webInterviewNotificationService.Object,
            CreateBinaryServices(Mock.Of<IImageFileStorage>(), audioFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "audio.aac")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/aac"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Audio(interviewId, questionIdentity.ToString(), "1", formFile));
        Assert.That(exception, Is.Not.Null);

        audioFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Once);
        audioFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, previousData, ContentTypeHelper.GetAudioContentType(fileName)), Times.Once);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public void when_uploading_audio_and_post_commit_exception_occurs_should_not_rollback_committed_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "audio__.aac";
        var previousData = new byte[] { 9, 8, 7 };
        DateTimeOffset? committedAnswerTime = null;

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "audio",
            QuestionType.Audio,
            null,
            null,
            null,
            false,
            false,
            false);

        var beforeUploadInterview = new Mock<IStatefulInterview>();
        beforeUploadInterview.SetupGet(x => x.Id).Returns(interviewId);
        beforeUploadInterview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        beforeUploadInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        beforeUploadInterview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(2)));

        var afterCommitInterview = new Mock<IStatefulInterview>();
        afterCommitInterview.SetupGet(x => x.Id).Returns(interviewId);
        afterCommitInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        afterCommitInterview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(() =>
            {
                var committedQuestion = new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(1));
                committedQuestion.SetAnswerTime(committedAnswerTime);
                return committedQuestion;
            });

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.SetupSequence(x => x.Get(It.IsAny<string>()))
            .Returns(beforeUploadInterview.Object)
            .Returns(beforeUploadInterview.Object)
            .Returns(afterCommitInterview.Object);

        var audioFileStorage = new Mock<IAudioFileStorage>();
        audioFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, fileName)).ReturnsAsync(previousData);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Callback<ICommand, string>((command, _) => committedAnswerTime = ((AnswerAudioQuestionCommand)command).OriginDate)
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            commandService.Object,
            webInterviewNotificationService.Object,
            CreateBinaryServices(Mock.Of<IImageFileStorage>(), audioFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "audio.aac")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/aac"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Audio(interviewId, questionIdentity.ToString(), "1", formFile));
        Assert.That(exception, Is.Not.Null);

        audioFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, It.IsAny<byte[]>(), "audio/aac"), Times.Once);
        audioFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Never);
        audioFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, previousData, ContentTypeHelper.GetAudioContentType(fileName)), Times.Never);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Never);
    }

    [Test]
    public void when_uploading_audio_and_post_commit_exception_occurs_with_same_duration_should_not_rollback_committed_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "audio__.aac";
        var previousData = new byte[] { 9, 8, 7 };
        DateTimeOffset? committedAnswerTime = null;

        var question = new InterviewTreeQuestion(
            questionIdentity,
            null,
            null,
            "audio",
            QuestionType.Audio,
            null,
            null,
            null,
            false,
            false,
            false);

        var beforeUploadInterview = new Mock<IStatefulInterview>();
        beforeUploadInterview.SetupGet(x => x.Id).Returns(interviewId);
        beforeUploadInterview.Setup(x => x.AcceptsInterviewerAnswers()).Returns(true);
        beforeUploadInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        beforeUploadInterview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(1)));

        var afterCommitInterview = new Mock<IStatefulInterview>();
        afterCommitInterview.SetupGet(x => x.Id).Returns(interviewId);
        afterCommitInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        afterCommitInterview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(() =>
            {
                var committedQuestion = new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(1));
                committedQuestion.SetAnswerTime(committedAnswerTime);
                return committedQuestion;
            });

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.SetupSequence(x => x.Get(It.IsAny<string>()))
            .Returns(beforeUploadInterview.Object)
            .Returns(beforeUploadInterview.Object)
            .Returns(afterCommitInterview.Object);

        var audioFileStorage = new Mock<IAudioFileStorage>();
        audioFileStorage.Setup(x => x.GetInterviewBinaryDataAsync(interviewId, fileName)).ReturnsAsync(previousData);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Callback<ICommand, string>((command, _) => committedAnswerTime = ((AnswerAudioQuestionCommand)command).OriginDate)
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new WebInterviewBinaryController(
            statefulInterviewRepository.Object,
            commandService.Object,
            webInterviewNotificationService.Object,
            CreateBinaryServices(Mock.Of<IImageFileStorage>(), audioFileStorage.Object),
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "audio.aac")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/aac"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Audio(interviewId, questionIdentity.ToString(), "1", formFile));
        Assert.That(exception, Is.Not.Null);

        audioFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, It.IsAny<byte[]>(), "audio/aac"), Times.Once);
        audioFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Never);
        audioFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, previousData, ContentTypeHelper.GetAudioContentType(fileName)), Times.Never);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Never);
    }

    private static WebInterviewBinaryServices CreateBinaryServices(IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage = null) =>
        new WebInterviewBinaryServices(
            Mock.Of<IImageProcessingService>(),
            audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
            Mock.Of<IAudioProcessingService>(),
            imageFileStorage);
}
