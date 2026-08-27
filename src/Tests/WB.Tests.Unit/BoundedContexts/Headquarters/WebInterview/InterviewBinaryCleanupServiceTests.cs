using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    [TestFixture]
    [TestOf(typeof(InterviewBinaryCleanupService))]
    public class InterviewBinaryCleanupServiceTests
    {
        [Test]
        public void when_processing_pending_cleanup_for_active_multimedia_file_should_not_delete_it()
        {
            var questionId = Id.g1;
            var userId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: questionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);
            const string fileName = "photo__.JPG";
            interview.AnswerPictureQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, fileName);

            IPlainStorageAccessor<InterviewBinaryCleanupRecord> storage = Create.Storage.InMemoryPlainStorage<InterviewBinaryCleanupRecord>();
            var record = new InterviewBinaryCleanupRecord
            {
                Id = Guid.NewGuid(),
                InterviewId = interview.Id,
                FileName = fileName,
                QuestionType = QuestionType.Multimedia,
                RequestedAt = DateTime.UtcNow,
                FailedCount = 1
            };
            storage.Store(record, record.Id);

            var imageFileStorage = new Mock<IImageFileStorage>();
            var service = CreateService(interview, storage, imageFileStorage: imageFileStorage.Object);

            service.ProcessPending(interview.Id);

            imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            Assert.That(storage.GetById(record.Id), Is.Null);
        }

        [Test]
        public void when_processing_pending_cleanup_for_stale_multimedia_file_should_delete_it()
        {
            var questionId = Id.g1;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: questionId, variable: "photo"));
            var interview = SetUp.StatefulInterview(questionnaire);
            const string fileName = "photo__.png";

            IPlainStorageAccessor<InterviewBinaryCleanupRecord> storage = Create.Storage.InMemoryPlainStorage<InterviewBinaryCleanupRecord>();
            var record = new InterviewBinaryCleanupRecord
            {
                Id = Guid.NewGuid(),
                InterviewId = interview.Id,
                FileName = fileName,
                QuestionType = QuestionType.Multimedia,
                RequestedAt = DateTime.UtcNow,
                FailedCount = 1
            };
            storage.Store(record, record.Id);

            var imageFileStorage = new Mock<IImageFileStorage>();
            imageFileStorage.Setup(x => x.RemoveInterviewBinaryData(interview.Id, fileName)).Returns(Task.CompletedTask);

            var service = CreateService(interview, storage, imageFileStorage: imageFileStorage.Object);

            service.ProcessPending(interview.Id);

            imageFileStorage.Verify(x => x.RemoveInterviewBinaryData(interview.Id, fileName), Times.Once);
            Assert.That(storage.GetById(record.Id), Is.Null);
        }

        private static InterviewBinaryCleanupService CreateService(
            WB.Core.SharedKernels.DataCollection.Aggregates.IStatefulInterview interview,
            IPlainStorageAccessor<InterviewBinaryCleanupRecord> storage,
            IImageFileStorage imageFileStorage = null,
            IAudioFileStorage audioFileStorage = null)
        {
            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(interview.Id.FormatGuid())).Returns(interview);

            return new InterviewBinaryCleanupService(
                storage,
                interviewRepository.Object,
                imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                audioFileStorage ?? Mock.Of<IAudioFileStorage>(),
                Mock.Of<ILogger<InterviewBinaryCleanupService>>());
        }
    }
}
