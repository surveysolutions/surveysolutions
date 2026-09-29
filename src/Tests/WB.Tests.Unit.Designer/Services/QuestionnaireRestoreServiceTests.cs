using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Services.Restore;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireRestoreService))]
    public class QuestionnaireRestoreServiceTests
    {
        [Test]
        public void When_archive_has_no_folder_for_a_referenced_attachment_restore_is_reported_as_failed()
        {
            var attachmentId = Guid.NewGuid();
            var document = DocumentWithAttachment(attachmentId);
            var service = CreateService(document);
            var state = new RestoreState();

            service.RestoreQuestionnaire(CreateArchive(("document.json", new byte[] { 1 })),
                Guid.NewGuid(), state, createNew: true);

            Assert.That(state.HasFailures, Is.True);
            Assert.That(state.Error, Does.Contain(attachmentId.FormatGuid()));
        }

        [Test]
        public void When_archive_has_only_part_of_a_referenced_attachment_restore_is_reported_as_failed()
        {
            var attachmentId = Guid.NewGuid();
            var document = DocumentWithAttachment(attachmentId);
            var service = CreateService(document);
            var state = new RestoreState();

            service.RestoreQuestionnaire(CreateArchive(
                    ("document.json", new byte[] { 1 }),
                    ($"attachments/{attachmentId:N}/content-type.txt", "image/png"u8.ToArray())),
                Guid.NewGuid(), state, createNew: true);

            Assert.That(state.HasFailures, Is.True);
            Assert.That(state.Error, Does.Contain(attachmentId.FormatGuid()));
        }

        [Test]
        public void When_archive_has_complete_attachment_restore_succeeds()
        {
            var attachmentId = Guid.NewGuid();
            var document = DocumentWithAttachment(attachmentId);
            var service = CreateService(document);
            var state = new RestoreState();

            service.RestoreQuestionnaire(CreateArchive(
                    ("document.json", new byte[] { 1 }),
                    ($"attachments/{attachmentId:N}/content-type.txt", "image/png"u8.ToArray()),
                    ($"attachments/{attachmentId:N}/logo.png", new byte[] { 2, 3 })),
                Guid.NewGuid(), state, createNew: true);

            Assert.That(state.HasFailures, Is.False);
        }

        private static QuestionnaireDocument DocumentWithAttachment(Guid attachmentId)
        {
            var document = new QuestionnaireDocument { PublicKey = Guid.NewGuid(), Title = "Questionnaire" };
            document.Attachments.Add(new Attachment { AttachmentId = attachmentId, Name = "logo" });
            return document;
        }

        private static QuestionnaireRestoreService CreateService(QuestionnaireDocument document)
        {
            var serializer = new Mock<ISerializer>();
            serializer.Setup(s => s.Deserialize<QuestionnaireDocument>(It.IsAny<string>())).Returns(document);

            var attachmentService = new Mock<IAttachmentService>();
            attachmentService.Setup(s => s.CreateAttachmentContentId(It.IsAny<byte[]>())).Returns("content-id");

            return new QuestionnaireRestoreService(
                NullLogger<QuestionnaireRestoreService>.Instance,
                serializer.Object,
                Mock.Of<ICommandService>(),
                Mock.Of<ILookupTableService>(),
                attachmentService.Object,
                Mock.Of<ITranslationsService>(),
                Create.InMemoryDbContext(),
                Mock.Of<IReusableCategoriesService>(),
                Mock.Of<IImportExportQuestionnaireMapper>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>());
        }

        private static Stream CreateArchive(params (string Path, byte[] Content)[] entries)
        {
            var stream = new MemoryStream();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (path, content) in entries)
                {
                    using var entryStream = archive.CreateEntry(path).Open();
                    entryStream.Write(content, 0, content.Length);
                }
            }

            stream.Position = 0;
            return stream;
        }
    }
}
