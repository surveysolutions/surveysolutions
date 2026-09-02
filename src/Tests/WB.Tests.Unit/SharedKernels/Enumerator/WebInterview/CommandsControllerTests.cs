using System;
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
    public void when_removing_picture_answer_should_remove_the_stored_file_with_its_original_extension()
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

        controller.RemoveAnswer(interviewId, new CommandsController.RemoveAnswerRequest { Identity = questionIdentity.ToString() });

        imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interviewId, fileName), Times.Once);
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
