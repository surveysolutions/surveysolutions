using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestFixture]
    [TestOf(typeof(CommandsController))]
    public class CommandsControllerTests
    {
        private static readonly Guid QuestionId = Id.g2;
        private static readonly Guid UserId = Id.gA;

        [Test]
        public async Task RemoveAnswer_when_multimedia_answer_exists_deletes_exact_stored_file_before_returning()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var existingFilename = "photo__.png";
            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, existingFilename);

            var commandService = new Mock<ICommandService>();
            var imageFileStorage = new Mock<IImageFileStorage>();
            var deleteCompletion = new TaskCompletionSource<bool>();

            imageFileStorage
                .Setup(s => s.RemoveInterviewBinaryData(interview.Id, existingFilename))
                .Returns(deleteCompletion.Task);

            var controller = CreateController(interview, questionnaire, commandService.Object, imageFileStorage.Object);
            var request = new CommandsController.RemoveAnswerRequest
            {
                Identity = Identity.Create(QuestionId, RosterVector.Empty).ToString()
            };

            var resultTask = controller.RemoveAnswer(interview.Id, request);

            Assert.That(resultTask.IsCompleted, Is.False);
            commandService.Verify(s => s.Execute(It.IsAny<RemoveAnswerCommand>(), null), Times.Once);
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(interview.Id, existingFilename), Times.Once);
            imageFileStorage.Verify(s => s.RemoveInterviewBinaryData(interview.Id, "photo__.jpg"), Times.Never);

            deleteCompletion.SetResult(true);

            var result = await resultTask;

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        private static TestCommandsController CreateController(
            StatefulInterview interview,
            IQuestionnaire questionnaire,
            ICommandService commandService = null,
            IImageFileStorage imageFileStorage = null)
        {
            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository
                .Setup(r => r.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(questionnaire);

            var statefulInterviewRepository = new Mock<IStatefulInterviewRepository>();
            statefulInterviewRepository
                .Setup(r => r.Get(interview.Id.FormatGuid()))
                .Returns(interview);

            return new TestCommandsController(
                commandService ?? Mock.Of<ICommandService>(),
                imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                Mock.Of<IAudioFileStorage>(),
                questionnaireRepository.Object,
                statefulInterviewRepository.Object,
                Mock.Of<IWebInterviewNotificationService>());
        }

        private class TestCommandsController : CommandsController
        {
            public TestCommandsController(
                ICommandService commandService,
                IImageFileStorage imageFileStorage,
                IAudioFileStorage audioFileStorage,
                IQuestionnaireStorage questionnaireRepository,
                IStatefulInterviewRepository statefulInterviewRepository,
                IWebInterviewNotificationService webInterviewNotificationService)
                : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService)
            {
            }

            public override IActionResult CompleteInterview(Guid interviewId, CompleteInterviewRequest completeInterviewRequest) => Ok();

            public override IActionResult PrepareCompleteInterview(Guid interviewId) => Ok();
        }
    }
}
