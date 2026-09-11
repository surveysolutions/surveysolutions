using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.Tests.Unit.SharedKernels.Enumerator.WebInterview;

[TestFixture]
[TestOf(typeof(CommandsController))]
public class CommandsControllerTests
{
    [Test]
    public async Task when_removing_picture_answer_should_remove_the_stored_file_with_its_original_extension()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "myfile.png";
        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.QuestionnaireIdentity).Returns(new QuestionnaireIdentity(Guid.NewGuid(), 1));
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(fileName, null));

        var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetQuestionType(questionIdentity.Id) == QuestionType.Multimedia);
        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(x =>
            x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x =>
            x.Get(It.IsAny<string>()) == interview.Object);
        var imageFileStorage = new Mock<IImageFileStorage>();
        var controller = new TestCommandsController(
            Mock.Of<ICommandService>(),
            imageFileStorage.Object,
            Mock.Of<IAudioFileStorage>(),
            questionnaireStorage,
            interviewRepository,
            Mock.Of<IWebInterviewNotificationService>());

        await controller.RemoveAnswer(interviewId, new CommandsController.RemoveAnswerRequest { Identity = questionIdentity.ToString() });

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Once);
    }

    [Test]
    public async Task when_removing_picture_answer_and_command_fails_should_not_remove_the_stored_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "myfile.png";
        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.QuestionnaireIdentity).Returns(new QuestionnaireIdentity(Guid.NewGuid(), 1));
        interview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(fileName, null));

        var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetQuestionType(questionIdentity.Id) == QuestionType.Multimedia);
        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(x =>
            x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x =>
            x.Get(It.IsAny<string>()) == interview.Object);
        var imageFileStorage = new Mock<IImageFileStorage>();
        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>())).Throws(new InvalidOperationException("Command failed"));

        var notificationService = new Mock<IWebInterviewNotificationService>();

        var controller = new TestCommandsController(
            commandService.Object,
            imageFileStorage.Object,
            Mock.Of<IAudioFileStorage>(),
            questionnaireStorage,
            interviewRepository,
            notificationService.Object);

        await controller.RemoveAnswer(interviewId, new CommandsController.RemoveAnswerRequest { Identity = questionIdentity.ToString() });

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Never);
        notificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public async Task when_loading_answer_metadata_fails_should_not_remove_the_answer()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var interviewRepository = new Mock<IStatefulInterviewRepository>();
        interviewRepository.Setup(x => x.Get(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Metadata load failed"));

        var commandService = new Mock<ICommandService>();
        var imageFileStorage = new Mock<IImageFileStorage>();
        var notificationService = new Mock<IWebInterviewNotificationService>();

        var controller = new TestCommandsController(
            commandService.Object,
            imageFileStorage.Object,
            Mock.Of<IAudioFileStorage>(),
            Mock.Of<IQuestionnaireStorage>(),
            interviewRepository.Object,
            notificationService.Object);

        await controller.RemoveAnswer(interviewId, new CommandsController.RemoveAnswerRequest { Identity = questionIdentity.ToString() });

        commandService.Verify(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()), Times.Never);
        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        notificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public async Task when_removing_picture_answer_and_post_commit_command_failure_occurs_should_remove_the_stored_file()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "myfile.png";
        var initialInterview = new Mock<IStatefulInterview>();
        initialInterview.SetupGet(x => x.QuestionnaireIdentity).Returns(new QuestionnaireIdentity(Guid.NewGuid(), 1));
        initialInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(fileName, null));

        var updatedInterview = new Mock<IStatefulInterview>();
        updatedInterview.SetupGet(x => x.QuestionnaireIdentity).Returns(initialInterview.Object.QuestionnaireIdentity);
        updatedInterview.Setup(x => x.GetMultimediaQuestion(questionIdentity))
            .Returns(new InterviewTreeMultimediaQuestion(null, null));

        var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetQuestionType(questionIdentity.Id) == QuestionType.Multimedia);
        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(x =>
            x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

        var interviewRepository = new Mock<IStatefulInterviewRepository>();
        interviewRepository.SetupSequence(x => x.Get(It.IsAny<string>()))
            .Returns(initialInterview.Object)
            .Returns(initialInterview.Object)
            .Returns(updatedInterview.Object);

        var imageFileStorage = new Mock<IImageFileStorage>();
        var commandService = new Mock<ICommandService>();
        commandService.Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Post-commit failure"));

        var notificationService = new Mock<IWebInterviewNotificationService>();

        var controller = new TestCommandsController(
            commandService.Object,
            imageFileStorage.Object,
            Mock.Of<IAudioFileStorage>(),
            questionnaireStorage,
            interviewRepository.Object,
            notificationService.Object);

        await controller.RemoveAnswer(interviewId, new CommandsController.RemoveAnswerRequest { Identity = questionIdentity.ToString() });

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Once);
        notificationService.Verify(x => x.MarkAnswerAsNotSaved(interviewId, questionIdentity, It.IsAny<Exception>()), Times.Never);
    }

    [Test]
    public async Task when_removing_audio_answer_should_remove_the_stored_file_with_its_original_filename()
    {
        var interviewId = Guid.NewGuid();
        var questionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
        var fileName = "myfile.aac";
        var interview = new Mock<IStatefulInterview>();
        interview.SetupGet(x => x.QuestionnaireIdentity).Returns(new QuestionnaireIdentity(Guid.NewGuid(), 1));
        interview.Setup(x => x.GetAudioQuestion(questionIdentity))
            .Returns(new InterviewTreeAudioQuestion(fileName, TimeSpan.FromSeconds(5)));

        var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetQuestionType(questionIdentity.Id) == QuestionType.Audio);
        var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(x =>
            x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x =>
            x.Get(It.IsAny<string>()) == interview.Object);
        var audioFileStorage = new Mock<IAudioFileStorage>();
        var controller = new TestCommandsController(
            Mock.Of<ICommandService>(),
            Mock.Of<IImageFileStorage>(),
            audioFileStorage.Object,
            questionnaireStorage,
            interviewRepository,
            Mock.Of<IWebInterviewNotificationService>());

        await controller.RemoveAnswer(interviewId, new CommandsController.RemoveAnswerRequest { Identity = questionIdentity.ToString() });

        audioFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Once);
    }

    private sealed class TestCommandsController : CommandsController
    {
        public TestCommandsController(
            ICommandService commandService,
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewNotificationService webInterviewNotificationService)
            : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository,
                webInterviewNotificationService)
        {
        }

        public override IActionResult CompleteInterview(Guid interviewId, CompleteInterviewRequest completeInterviewRequest) => Ok();

        public override IActionResult PrepareCompleteInterview(Guid interviewId) => Ok();
    }
}
