using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;
using WB.Tests.Abc;
using WB.UI.Headquarters.Services;
using File = WB.UI.Headquarters.Services.File;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.QuestionnaireExporterTest
{
    public class when_export_simple_questionnaire_without_additions : QuestionnaireExporterSpecification
    {
        protected override void Context() { 
        }

        [Test]
        public void should_export_zip_with_single_entry() => Assert.That(zipArchive.Entries, Has.Count.EqualTo(1));
    }

    public class when_export_questionnaire_with_broken_attachments : QuestionnaireExporterSpecification
    {
        private static readonly Guid BrokenAttachment = Id.g1;
        private static readonly Guid MissingAttachment = Id.g2;

        protected override void Context()
        {
            MockOf<IAttachmentContentService>()
                .Setup(c => c.GetAttachmentContent("Missing"))
                .Returns((AttachmentContent)null);

            questionnaireDocument.Attachments.Add(
                new Attachment
                {
                    AttachmentId = MissingAttachment,
                    ContentId = "Missing"
                });

            MockOf<IAttachmentContentService>()
                .Setup(c => c.GetAttachmentContent("Broken"))
                .Throws(new Exception("Attachment is broken"));

            questionnaireDocument.Attachments.Add(
                new Attachment
                {
                    AttachmentId = BrokenAttachment,
                    ContentId = "Broken"
                });
        }

        [Test]
        public void should_contain_missing_attachment_info()
        {
            var infoPath = $"Attachments/Invalid/missing attachment #1 ({MissingAttachment.FormatGuid()}).txt";
            AssertZipContainsFile(infoPath);
        }

        [Test]
        public void should_contain_broken_attachment_info()
        {
            var infoPath = $"Attachments/Invalid/broken attachment #2.txt";
            AssertZipContainsFile(infoPath);
        }
    }

    public class when_export_questionnaire_with_lookup_tables : QuestionnaireExporterSpecification
    {
        protected override void Context()
        {
            questionnaireDocument.LookupTables.Add(Id.g1, new LookupTable());

            MockOf<IPlainKeyValueStorage<QuestionnaireLookupTable>>()
                .Setup(s => s.GetById(It.Is<string>(i => i.EndsWith(Id.g1.FormatGuid()))))
                .Returns(new QuestionnaireLookupTable
                {
                    Content = Convert.ToBase64String(Encoding.UTF8.GetBytes("SampleContent"))
                });
        }

        [Test]
        public void should_export_lookup_tables()
        {
            AssertZipContainsFile($"Lookup Tables/{Id.g1.FormatGuid()}.txt");
        }

        [Test]
        public void should_export_lookup_table_content()
        {
            var text = GetTextContentFromZip($"Lookup Tables/{Id.g1.FormatGuid()}.txt");
            Assert.That(text, Is.EqualTo("SampleContent"));
        }
    }

    public class when_export_questionnaire_with_translations : QuestionnaireExporterSpecification
    {
        protected override void Context()
        {
            questionnaireDocument.Translations.Add(new Translation
            {
                Id = Id.g1, Name = "SampleTranslation"
            });

            fixture.Freeze<ITranslationManagementService>();
            fixture.Freeze<ITranslationsExportService>();
        }

        [Test]
        public void should_export_translation()
        {
            AssertZipContainsFile($"Translations/{Id.g1}.xlsx");
        }

        [Test]
        public void should_generate_export_translation()
        {
            MockOf<ITranslationsExportService>().Verify(s => s.GenerateTranslationFile(questionnaireDocument, Id.g1, It.IsAny<ITranslation>()), Times.Once);
        }

        [Test]
        public void should_query_proper_translations()
        {
            MockOf<ITranslationManagementService>()
                .Verify(s => s.GetAll(questionnaireIdentity, Id.g1), Times.Once);
        }
    }

    public class when_export_questionnaire_with_attachments : QuestionnaireExporterSpecification
    {
        private static readonly List<AttachmentSpecification> attachments = new List<AttachmentSpecification>{
            new AttachmentSpecification(Id.g1, "Hash1", "image/jpg", "ContentOfHash1"),
            new AttachmentSpecification(Id.g2, "Hash2", "image/png", "ContentOfHash2")
        };

        protected override void Context()
        {
            foreach (var attachment in attachments)
            {
                MockOf<IAttachmentContentService>()
                    .Setup(c => c.GetAttachmentContent(attachment.Hash))
                    .Returns(new AttachmentContent
                    {
                        Content = Encoding.UTF8.GetBytes(attachment.Content),
                        ContentHash = attachment.Hash,
                        ContentType = attachment.ContentType
                    });

                questionnaireDocument.Attachments.Add(
                    new Attachment
                    {
                        AttachmentId = attachment.Id,
                        ContentId = attachment.Hash,
                        Name = attachment.Hash
                    });
            }
        }

        [TestCaseSource(nameof(attachments))]
        public void should_add_attachment_extension_based_on_content_type(AttachmentSpecification attachment)
        {
            var entry = GetAttachmentFolder(attachment.Id) + $"/{attachment.Hash}.{attachment.ExpectedExtension}";
            AssertZipContainsFile(entry);
        }

        [TestCaseSource(nameof(attachments))]
        public void should_add_attachment_contentType_txt_with_content_type_info(AttachmentSpecification attachment)
        {
            var contentTypePath = GetAttachmentFolder(attachment.Id) + $"/Content-Type.txt";
            AssertZipContainsFile(contentTypePath);

            var contentTypeValue = GetTextContentFromZip(contentTypePath);
            Assert.That(contentTypeValue, Is.EqualTo(attachment.ContentType));
        }

        public class AttachmentSpecification
        {
            public AttachmentSpecification(Guid id, string hash, string contentContentType, string content)
            {
                Id = id;
                Hash = hash;
                ContentType = contentContentType;
                Content = content;
                ExpectedExtension = contentContentType.Split('/').Last();
            }

            public Guid Id { get; set; }
            public string Hash { get; set; }
            public string ContentType { get; set; }
            public string Content { get; set; }
            public string ExpectedExtension { get; set; }

            public override string ToString()
            {
                return $"{Id}/{Hash} {ContentType} {Content}";
            }
        }

        private string GetAttachmentFolder(Guid attachmentId) => "Attachments/" + attachmentId.FormatGuid();
    }

    [TestFixture]
    public abstract class QuestionnaireExporterSpecification
    {
        protected Fixture fixture;
        protected File fileResponse;
        protected ZipArchive zipArchive;
        protected QuestionnaireDocument questionnaireDocument;
        protected QuestionnaireIdentity questionnaireIdentity;
        protected const string SerializedQuestionnaireDocument = "QuestionnaireDocumentJsonContent";

        [OneTimeSetUp]
        public void Establish()
        {
            this.fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());

            this.questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.gA, 1);
            this.questionnaireDocument = Create.Entity.QuestionnaireDocument(id: Id.gA);

            Context();

            questionnaireDocument.Title = "SampleTitle";
            questionnaireDocument.VariableName = "sample_title";

            fixture.Register(() => questionnaireDocument);

            MockOf<IEntitySerializer<QuestionnaireDocument>>()
                .Setup(s => s.Serialize(It.IsAny<QuestionnaireDocument>()))
                .Returns(SerializedQuestionnaireDocument);

            var subj = fixture.Create<QuestionnaireExporter>();

            this.fileResponse = subj.CreateZipExportFile(questionnaireIdentity);
            this.zipArchive = new ZipArchive(fileResponse.FileStream);
        }

        protected abstract void Context();

        protected Mock<T> MockOf<T>() where T : class
        {
            return Mock.Get(fixture.Freeze<T>());
        }

        [Test]
        public void should_put_all_files_into_folder_with_questionnaire_name()
        {
            var questionnaireFolder = GetQuestionnaireFolderName();
            Assert.That(zipArchive.Entries, Has.All.Property(nameof(ZipArchiveEntry.FullName)).StartsWith(questionnaireFolder));
        }

        [Test]
        public void should_put_questionnaire_json_into_zip()
        {
            AssertZipContainsFile(questionnaireDocument.VariableName + ".json");
        }

        [Test]
        public void should_put_questionnaire_json_content_into_zip() =>
            Assert.That(GetTextContentFromZip(questionnaireDocument.VariableName + ".json"), Is.EqualTo(SerializedQuestionnaireDocument));

        protected void AssertZipContainsFile(string path)
        {
            Assert.That(zipArchive.Entries,
                Has.One.Property(nameof(ZipArchiveEntry.FullName))
                    .EqualTo($"{GetQuestionnaireFolderName()}/{path}"));
        }

        protected string GetQuestionnaireFolderName() =>
            $"{questionnaireDocument.VariableName} ({questionnaireIdentity.QuestionnaireId.FormatGuid()})";

        protected ZipArchiveEntry GetEntry(string path = null)
            => zipArchive.GetEntry($"{GetQuestionnaireFolderName()}/{ path ?? string.Empty}");

        protected string GetTextContentFromZip(string path)
        {
            var contentStream = GetEntry(path)?.Open();
            if (contentStream == null) return null;
            using (contentStream)
            {
                using (var ms = new MemoryStream())
                {
                    contentStream.CopyTo(ms);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
