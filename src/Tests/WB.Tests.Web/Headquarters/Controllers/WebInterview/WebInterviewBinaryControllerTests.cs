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
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.Controllers;
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
            Mock.Of<IImageProcessingService>(),
            Mock.Of<IWebInterviewNotificationService>(),
            Mock.Of<IAudioFileStorage>(),
            Mock.Of<IAudioProcessingService>(),
            imageFileStorage.Object,
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
            Mock.Of<IImageProcessingService>(),
            Mock.Of<IWebInterviewNotificationService>(),
            Mock.Of<IAudioFileStorage>(),
            Mock.Of<IAudioProcessingService>(),
            imageFileStorage.Object,
            Mock.Of<ILogger<WebInterviewBinaryController>>());

        var bytes = new byte[] { 1, 2, 3 };
        var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "newphoto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        await controller.Image(interviewId, questionIdentity.ToString(), formFile);

        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, "photo__.png", It.IsAny<byte[]>(), "image/png"), Times.Once);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, "photo__.png"), Times.Never);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, oldFileName), Times.Once);
    }

    [Test]
    public async Task when_uploading_picture_with_case_only_extension_change_and_command_fails_should_restore_original_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var oldFileName = "photo__.PNG";
        var newFileName = "photo__.png";
        var oldData = new byte[] { 7, 8, 9 };

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
            Mock.Of<IImageProcessingService>(),
            Mock.Of<IWebInterviewNotificationService>(),
            Mock.Of<IAudioFileStorage>(),
            Mock.Of<IAudioProcessingService>(),
            imageFileStorage.Object,
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
        imageFileStorage.Verify(x => x.StoreInterviewBinaryData(interviewId, oldFileName, oldData, "image/png"), Times.Once);
    }
}
