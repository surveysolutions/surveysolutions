using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer
{
    [TestFixture]
    public class AttachmentsCleanupServiceTests : MvxIoCSupportingTest
    {
        private IPlainStorage<AttachmentContentMetadata> metadataStorage;
        private IPlainStorage<AttachmentContentData> contentStorage;
        protected List<QuestionnaireDocumentView> questionnaires = new List<QuestionnaireDocumentView>();
        private IInterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;

        [Test]
        public void when_no_questionnaires_use_attchament_it_should_be_removed()
        {
            var contentId = "meta";
            metadataStorage.Store(new AttachmentContentMetadata { Id = contentId, ContentType = "application/json", Size = 4 });
            contentStorage.Store(new AttachmentContentData {Content = new byte[] {1, 2, 3}, Id = contentId});

            var service = this.CreateAttachmentsCleanupService(metadataStorage: metadataStorage,
                contentStorage: contentStorage, questionnairesAccessor: interviewerQuestionnaireAccessor);

            service.RemovedOrphanedAttachments();

            var dbMetaAfterRemoval = metadataStorage.GetById(contentId);
            var dbDataAfterRemoval = contentStorage.GetById(contentId);

            Assert.That(dbMetaAfterRemoval, Is.Null);
            Assert.That(dbDataAfterRemoval, Is.Null);
        }

        public AttachmentsCleanupServiceTests()
        {
            base.Setup();
        }

        [SetUp]
        public void TestSetup()
        {
            this.metadataStorage = new SqliteInmemoryStorage<AttachmentContentMetadata>();
            this.contentStorage = new SqliteInmemoryStorage<AttachmentContentData>();

            this.interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(x => x.LoadAll() == this.questionnaires);
        }

        [Test]
        public void when_questionnaire_uses_attachment_it_should_not_be_removed()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithAttachments(null, Create.Entity.Attachment("meta"));

            Ioc.RegisterSingleton(Mock.Of<IJsonAllTypesSerializer>(s =>
                s.Deserialize<QuestionnaireDocument>(It.IsAny<byte[]>()) == questionnaire
                && s.Serialize(It.IsAny<QuestionnaireDocument>()) == "any"));

            this.questionnaires.Add(new QuestionnaireDocumentView { QuestionnaireDocument = questionnaire });

            var contentId = "meta";
            metadataStorage.Store(new AttachmentContentMetadata { Id = contentId, ContentType = "application/json", Size = 4 });
            contentStorage.Store(new AttachmentContentData { Content = new byte[] { 1, 2, 3 }, Id = contentId });
            
            var service = this.CreateAttachmentsCleanupService(metadataStorage: metadataStorage,
                contentStorage: contentStorage,
                questionnairesAccessor: interviewerQuestionnaireAccessor);

            service.RemovedOrphanedAttachments();

            var dbMetaAfterRemoval = metadataStorage.GetById(contentId);
            var dbDataAfterRemoval = contentStorage.GetById(contentId);

            Assert.That(dbMetaAfterRemoval, Is.Not.Null);
            Assert.That(dbDataAfterRemoval, Is.Not.Null);
        }

        [Test]
        [Ignore("KP-12123")]
        public void when_questionnaire_has_stored_file_it_should_be_removed()
        {
            var contentId = "meta";
            var contentFile = contentId + ".attachment";
            metadataStorage.Store(new AttachmentContentMetadata { Id = contentId, ContentType = "video/data", Size = 4 });
            contentStorage.Store(new AttachmentContentData { Content = new byte[] { 1, 2, 3 }, Id = contentId });

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
        
            fileSystemAccessor.Setup(fs => fs.IsFileExists(It.Is<string>(f => f.EndsWith(contentFile)))).Returns(true);
            
            var service = this.CreateAttachmentsCleanupService(
                metadataStorage: metadataStorage,
                contentStorage: contentStorage,
                questionnairesAccessor: interviewerQuestionnaireAccessor,
                fileSystemAccessor.Object);

            // act
            service.RemovedOrphanedAttachments();

            // assert
            fileSystemAccessor.Verify(fs => fs.DeleteFile(It.Is<string>(f => f.EndsWith(contentFile))), Times.Once);

            var dbMetaAfterRemoval = metadataStorage.GetById(contentId);
            var dbDataAfterRemoval = contentStorage.GetById(contentId);

            Assert.That(dbMetaAfterRemoval, Is.Null);
            Assert.That(dbDataAfterRemoval, Is.Null);
        }

        AttachmentsCleanupService CreateAttachmentsCleanupService(
            IPlainStorage<AttachmentContentMetadata> metadataStorage = null,
            IPlainStorage<AttachmentContentData> contentStorage = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            var metadata = metadataStorage ?? new SqliteInmemoryStorage<AttachmentContentMetadata>();
            var content = contentStorage ?? new SqliteInmemoryStorage<AttachmentContentData>();
            var files = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();

            return new AttachmentsCleanupService(questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                metadata,
                Create.Service.AttachmentContentStorage(metadata, content, files: files),
                Mock.Of<ILogger>());
        }
    }
}
