using System;
using System.IO;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Controllers;
using WB.UI.WebTester.Services;

namespace WB.Tests.Web.WebTester.Controllers;

[TestFixture]
[TestOf(typeof(MultimediaController))]
public class MultimediaControllerTests
{
    [Test]
    public void when_uploading_audio_and_post_commit_exception_occurs_should_not_rollback_committed_file()
    {
        var interviewId = Guid.NewGuid();
        var interviewIdString = interviewId.FormatGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "audio__.aac";
        var previousFile = new MultimediaFile(fileName, new byte[] { 9, 8, 7 }, TimeSpan.FromSeconds(1), "audio/aac");
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
        statefulInterviewRepository.SetupSequence(x => x.Get(interviewIdString))
            .Returns(beforeUploadInterview.Object)
            .Returns(beforeUploadInterview.Object)
            .Returns(afterCommitInterview.Object);

        var mediaStorage = new Mock<ICacheStorage<MultimediaFile, string>>();
        mediaStorage.Setup(x => x.Get(fileName, interviewId)).Returns(previousFile);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Callback<ICommand, string>((command, _) => committedAnswerTime = ((AnswerAudioQuestionCommand)command).OriginDate)
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new MultimediaController(
            commandService.Object,
            statefulInterviewRepository.Object,
            webInterviewNotificationService.Object,
            mediaStorage.Object,
            Mock.Of<IAudioProcessingService>(),
            Mock.Of<IImageProcessingService>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "audio.aac")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/aac"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Audio(interviewIdString, questionIdentity.ToString(), "1", formFile));
        Assert.That(exception, Is.Not.Null);

        mediaStorage.Verify(x => x.Store(It.Is<MultimediaFile>(file => file.Filename == fileName && file.MimeType == "audio/aac"), fileName, interviewId), Times.Once);
        mediaStorage.Verify(x => x.Store(previousFile, fileName, interviewId), Times.Never);
        mediaStorage.Verify(x => x.Remove(fileName, interviewId), Times.Never);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Never);
    }

    [Test]
    public void when_uploading_picture_and_post_commit_exception_occurs_should_cleanup_old_file_without_marking_unsaved()
    {
        var interviewId = Guid.NewGuid();
        var interviewIdString = interviewId.FormatGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.jpg";
        var newFileName = "photo__.png";
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
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, DateTime.UtcNow.AddMinutes(-1)));

        var afterCommitInterview = new Mock<IStatefulInterview>();
        afterCommitInterview.SetupGet(x => x.Id).Returns(interviewId);
        afterCommitInterview.Setup(x => x.GetQuestion(questionIdentity)).Returns(question);
        afterCommitInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(() => new InterviewTreeMultimediaQuestion(newFileName, committedAnswerTimeUtc));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.SetupSequence(x => x.Get(interviewIdString))
            .Returns(beforeUploadInterview.Object)
            .Returns(beforeUploadInterview.Object)
            .Returns(afterCommitInterview.Object);

        var mediaStorage = new Mock<ICacheStorage<MultimediaFile, string>>();

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Callback<ICommand, string>((command, _) => committedAnswerTimeUtc = ((AnswerPictureQuestionCommand)command).OriginDate.UtcDateTime)
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new MultimediaController(
            commandService.Object,
            statefulInterviewRepository.Object,
            webInterviewNotificationService.Object,
            mediaStorage.Object,
            Mock.Of<IAudioProcessingService>(),
            Mock.Of<IImageProcessingService>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Image(interviewIdString, questionIdentity.ToString(), formFile));
        Assert.That(exception, Is.Not.Null);

        mediaStorage.Verify(x => x.Store(It.Is<MultimediaFile>(file =>
            file.Filename == newFileName
            && file.MimeType == "image/png"
            && file.Data.Length == bytes.Length
            && file.Data[0] == bytes[0]), newFileName, interviewId), Times.Once);
        mediaStorage.Verify(x => x.Remove(oldFileName, interviewId), Times.Once);
        mediaStorage.Verify(x => x.Remove(newFileName, interviewId), Times.Never);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Never);
    }

    [Test]
    public void when_uploading_picture_and_command_is_not_committed_should_restore_previous_file_by_new_name_and_mark_unsaved()
    {
        var interviewId = Guid.NewGuid();
        var interviewIdString = interviewId.FormatGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.jpg";
        var newFileName = "photo__.png";
        var previousFileByNewName = new MultimediaFile(newFileName, new byte[] { 7, 8, 9 }, null, "image/png");

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
            .Returns(new InterviewTreeMultimediaQuestion(oldFileName, DateTime.UtcNow.AddMinutes(-1)));

        var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
        statefulInterviewRepository.Setup(x => x.Get(interviewIdString)).Returns(interview.Object);

        var mediaStorage = new Mock<ICacheStorage<MultimediaFile, string>>();
        mediaStorage.Setup(x => x.Get(newFileName, interviewId)).Returns(previousFileByNewName);

        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("boom"));

        var webInterviewNotificationService = new Mock<IWebInterviewNotificationService>();
        var controller = new MultimediaController(
            commandService.Object,
            statefulInterviewRepository.Object,
            webInterviewNotificationService.Object,
            mediaStorage.Object,
            Mock.Of<IAudioProcessingService>(),
            Mock.Of<IImageProcessingService>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Image(interviewIdString, questionIdentity.ToString(), formFile));
        Assert.That(exception, Is.Not.Null);

        mediaStorage.Verify(x => x.Store(It.Is<MultimediaFile>(file =>
            file.Filename == newFileName
            && file.MimeType == "image/png"
            && file.Data.Length == bytes.Length
            && file.Data[0] == bytes[0]), newFileName, interviewId), Times.Once);
        mediaStorage.Verify(x => x.Store(previousFileByNewName, newFileName, interviewId), Times.Once);
        mediaStorage.Verify(x => x.Remove(newFileName, interviewId), Times.Never);
        webInterviewNotificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Once);
    }
}
