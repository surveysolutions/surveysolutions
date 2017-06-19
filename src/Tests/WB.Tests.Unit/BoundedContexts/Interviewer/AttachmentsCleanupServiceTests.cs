﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer
{
    [TestFixture]
    public class AttachmentsCleanupServiceTests : MvvmCross.Test.Core.MvxIoCSupportingTest
    {
        [Test]
        public void when_no_questionnaires_use_attchament_it_should_be_removed()
        {
            IPlainStorage<AttachmentContentMetadata> metadataStorage = new SqliteInmemoryStorage<AttachmentContentMetadata>();
            IPlainStorage<AttachmentContentData> contentStorage = new SqliteInmemoryStorage<AttachmentContentData>();

            var contentId = "meta";
            metadataStorage.Store(new AttachmentContentMetadata { Id = contentId, ContentType = "application/json", Size = 4 });
            contentStorage.Store(new AttachmentContentData {Content = new byte[] {1, 2, 3}, Id = contentId});

            var service = this.CreateAttachmentsCleanupService(metadataStorage: metadataStorage,
                contentStorage: contentStorage);

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

        [Test]
        public void when_questionnaire_uses_attachment_it_should_not_be_removed()
        {
            IPlainStorage<AttachmentContentMetadata> metadataStorage = new SqliteInmemoryStorage<AttachmentContentMetadata>();
            IPlainStorage<AttachmentContentData> contentStorage = new SqliteInmemoryStorage<AttachmentContentData>();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithAttachments(null, Create.Entity.Attachment("meta"));

            Ioc.RegisterSingleton(Mock.Of<IJsonAllTypesSerializer>(s =>
                s.Deserialize<QuestionnaireDocument>(It.IsAny<byte[]>()) == questionnaire
                && s.Serialize(It.IsAny<QuestionnaireDocument>()) == "any"));

            var documentView = new QuestionnaireDocumentView { QuestionnaireDocument = questionnaire };

            var contentId = "meta";
            metadataStorage.Store(new AttachmentContentMetadata { Id = contentId, ContentType = "application/json", Size = 4 });
            contentStorage.Store(new AttachmentContentData { Content = new byte[] { 1, 2, 3 }, Id = contentId });

            IInterviewerQuestionnaireAccessor questionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.LoadAll() == new List<QuestionnaireDocumentView> { documentView });

            var service = this.CreateAttachmentsCleanupService(metadataStorage: metadataStorage,
                contentStorage: contentStorage,
                questionnairesAccessor: questionnaireAccessor);

            service.RemovedOrphanedAttachments();

            var dbMetaAfterRemoval = metadataStorage.GetById(contentId);
            var dbDataAfterRemoval = contentStorage.GetById(contentId);

            Assert.That(dbMetaAfterRemoval, Is.Not.Null);
            Assert.That(dbDataAfterRemoval, Is.Not.Null);
        }

        AttachmentsCleanupService CreateAttachmentsCleanupService(IPlainStorage<AttachmentContentMetadata> metadataStorage = null,
            IPlainStorage<AttachmentContentData> contentStorage = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null)
        {
            return new AttachmentsCleanupService(questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                metadataStorage ?? new SqliteInmemoryStorage<AttachmentContentMetadata>(),
                contentStorage ?? new SqliteInmemoryStorage<AttachmentContentData>(),
                Mock.Of<ILogger>());
        }
    }
}