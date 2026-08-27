using System;
using System.Collections.Generic;
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
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
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
            var imageFileStorage = new BlockingImageFileStorage();
            var commandExecuted = new TaskCompletionSource<bool>();

            commandService
                .Setup(s => s.Execute(It.IsAny<ICommand>(), null))
                .Callback(() => commandExecuted.SetResult(true));

            var controller = CreateController(interview, questionnaire, commandService.Object, imageFileStorage);
            var request = new CommandsController.RemoveAnswerRequest
            {
                Identity = Identity.Create(QuestionId, RosterVector.Empty).ToString()
            };

            var resultTask = Task.Run(async () => await controller.RemoveAnswer(interview.Id, request));
            await commandExecuted.Task;
            await imageFileStorage.RemoveStarted.Task;

            Assert.That(resultTask.IsCompleted, Is.False);
            commandService.Verify(s => s.Execute(It.IsAny<RemoveAnswerCommand>(), null), Times.Once);
            Assert.That(imageFileStorage.RemovedInterviewId, Is.EqualTo(interview.Id));
            Assert.That(imageFileStorage.RemovedFileName, Is.EqualTo(existingFilename));

            imageFileStorage.RemoveCompletion.SetResult(true);

            var result = await resultTask;

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task RemoveAnswer_when_binary_cleanup_fails_does_not_mark_answer_as_not_saved()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: QuestionId, variable: "photo"));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = SetUp.StatefulInterview(questionnaireDocument);
            var existingFilename = "photo__.png";
            interview.AnswerPictureQuestion(UserId, QuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, existingFilename);

            var imageFileStorage = new Mock<IImageFileStorage>();
            imageFileStorage
                .Setup(x => x.RemoveInterviewBinaryData(interview.Id, existingFilename))
                .ThrowsAsync(new Exception("storage failure"));

            var notificationService = new Mock<IWebInterviewNotificationService>();
            var cleanupService = new Mock<IInterviewBinaryCleanupService>();

            var controller = CreateController(
                interview,
                questionnaire,
                imageFileStorage: imageFileStorage.Object,
                webInterviewNotificationService: notificationService.Object,
                interviewBinaryCleanupService: cleanupService.Object);

            var request = new CommandsController.RemoveAnswerRequest
            {
                Identity = Identity.Create(QuestionId, RosterVector.Empty).ToString()
            };

            var result = await controller.RemoveAnswer(interview.Id, request);

            Assert.That(result, Is.InstanceOf<OkResult>());
            imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interview.Id, existingFilename), Times.Once);
            notificationService.Verify(x => x.MarkAnswerAsNotSaved(It.IsAny<Guid>(), It.IsAny<Identity>(), It.IsAny<Exception>()), Times.Never);
            notificationService.Verify(x => x.MarkAnswerAsNotSaved(It.IsAny<Guid>(), It.IsAny<Identity>(), It.IsAny<string>()), Times.Never);
            cleanupService.Verify(x => x.EnqueueImageCleanup(interview.Id, existingFilename, It.IsAny<Exception>()), Times.Once);
        }

        private static TestCommandsController CreateController(
            StatefulInterview interview,
            IQuestionnaire questionnaire,
            ICommandService commandService = null,
            IImageFileStorage imageFileStorage = null,
            IWebInterviewNotificationService webInterviewNotificationService = null,
            IInterviewBinaryCleanupService interviewBinaryCleanupService = null)
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
                webInterviewNotificationService ?? Mock.Of<IWebInterviewNotificationService>(),
                interviewBinaryCleanupService ?? Mock.Of<IInterviewBinaryCleanupService>(),
                Stub.Lock());
        }

        private class BlockingImageFileStorage : IImageFileStorage
        {
            public Guid? RemovedInterviewId { get; private set; }
            public string RemovedFileName { get; private set; }
            public TaskCompletionSource<bool> RemoveStarted { get; } = new();
            public TaskCompletionSource<bool> RemoveCompletion { get; } = new();

            public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName) => Task.FromResult<byte[]>(null);

            public byte[] GetInterviewBinaryData(Guid interviewId, string fileName) => null;

            public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId) =>
                Task.FromResult(new List<InterviewBinaryDataDescriptor>());

            public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
            {
            }

            public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
            {
                this.RemovedInterviewId = interviewId;
                this.RemovedFileName = fileName;
                this.RemoveStarted.SetResult(true);
                return this.RemoveCompletion.Task;
            }

            public string GetPath(Guid interviewId, string filename = null) => filename;

            public Task RemoveAllBinaryDataForInterviewsAsync(List<Guid> interviewIds) => Task.CompletedTask;
        }

        private class TestCommandsController : CommandsController
        {
            public TestCommandsController(
                ICommandService commandService,
                IImageFileStorage imageFileStorage,
                IAudioFileStorage audioFileStorage,
                IQuestionnaireStorage questionnaireRepository,
                IStatefulInterviewRepository statefulInterviewRepository,
                IWebInterviewNotificationService webInterviewNotificationService,
                IInterviewBinaryCleanupService interviewBinaryCleanupService,
                WB.Core.Infrastructure.Aggregates.IAggregateLock aggregateLock)
                : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService, interviewBinaryCleanupService, aggregateLock)
            {
            }

            public override IActionResult CompleteInterview(Guid interviewId, CompleteInterviewRequest completeInterviewRequest) => Ok();

            public override IActionResult PrepareCompleteInterview(Guid interviewId) => Ok();
        }
    }
}
