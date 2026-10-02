using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.Verifier
{
    [TestFixture]
    [TestOf(typeof(AttachmentVerifications))]
    public class AttachmentVerificationsTests
    {
        private const long Mb = 1024 * 1024;

        private Mock<IAttachmentService> attachmentService;
        private Mock<IKeywordsProvider> keywords;
        private AttachmentVerifications verifier;
        private readonly Guid questionnaireId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            attachmentService = new Mock<IAttachmentService>();
            keywords = new Mock<IKeywordsProvider>();
            attachmentService.Setup(a => a.GetContentDetails(It.IsAny<string>()))
                .Returns(new AttachmentContent { Size = 10 });
            attachmentService.Setup(a => a.GetAttachmentSizesByQuestionnaire(It.IsAny<Guid>()))
                .Returns(new List<AttachmentSize>());
            verifier = new AttachmentVerifications(attachmentService.Object, keywords.Object);
        }

        private static Attachment Att(string name, Guid? id = null, string contentId = "content")
            => new Attachment { AttachmentId = id ?? Guid.NewGuid(), Name = name, ContentId = contentId };

        private List<QuestionnaireVerificationMessage> Verify(params Attachment[] attachments)
        {
            var doc = Create.QuestionnaireDocumentWithoutChildren(questionnaireId, attachments);
            var multi = new MultiLanguageQuestionnaireDocument(
                new ReadOnlyQuestionnaireDocumentWithCache(doc),
                Enumerable.Empty<ReadOnlyQuestionnaireDocumentWithCache>(),
                Enumerable.Empty<SharedPersonView>());
            return verifier.Verify(multi).ToList();
        }

        [Test]
        public void Valid_attachment_should_produce_no_messages()
        {
            Verify(Att("valid_name1")).Should().BeEmpty();
        }

        [Test]
        public void Missing_content_should_produce_WB0111()
        {
            attachmentService.Setup(a => a.GetContentDetails("content")).Returns((AttachmentContent)null);
            var att = Att("valid");

            var messages = Verify(att);

            messages.Should().ContainSingle(m => m.Code == "WB0111")
                .Which.References.Single().Id.Should().Be(att.AttachmentId);
        }

        [Test]
        public void Zero_size_content_should_produce_WB0111()
        {
            attachmentService.Setup(a => a.GetContentDetails("content")).Returns(new AttachmentContent { Size = 0 });

            Verify(Att("valid")).Should().Contain(m => m.Code == "WB0111");
        }

        [TestCase("")]
        [TestCase("1abc")]
        [TestCase("_abc")]
        [TestCase("abc_")]
        [TestCase("a__b")]
        [TestCase("a b")]
        [TestCase("a-b")]
        public void Invalid_names_should_produce_WB0315(string name)
        {
            Verify(Att(name)).Should().Contain(m => m.Code == "WB0315");
        }

        [Test]
        public void Too_long_name_should_produce_WB0315()
        {
            Verify(Att(new string('a', 33))).Should().Contain(m => m.Code == "WB0315");
        }

        [Test]
        public void Name_of_exact_limit_should_be_valid()
        {
            Verify(Att(new string('a', 32))).Should().NotContain(m => m.Code == "WB0315");
        }

        [Test]
        public void Reserved_keyword_name_should_produce_WB0315()
        {
            keywords.Setup(k => k.IsReservedKeyword("class")).Returns(true);

            Verify(Att("class")).Should().Contain(m => m.Code == "WB0315");
        }

        [Test]
        public void Duplicate_names_should_produce_WB0065_ignoring_case()
        {
            var first = Att("Photo");
            var second = Att("photo");

            var messages = Verify(first, second);

            messages.Should().ContainSingle(m => m.Code == "WB0065")
                .Which.References.Select(r => r.Id).Should().BeEquivalentTo(new[] { first.AttachmentId, second.AttachmentId });
        }

        [Test]
        public void Attachment_over_5mb_should_produce_warning_WB0213()
        {
            var att = Att("big");
            attachmentService.Setup(a => a.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(new List<AttachmentSize> { new AttachmentSize { AttachmentId = att.AttachmentId, Size = 5 * Mb + 1 } });

            var message = Verify(att).Single(m => m.Code == "WB0213");

            message.MessageLevel.Should().Be(VerificationMessageLevel.Warning);
        }

        [Test]
        public void Attachment_of_exactly_5mb_should_not_warn()
        {
            var att = Att("ok");
            attachmentService.Setup(a => a.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(new List<AttachmentSize> { new AttachmentSize { AttachmentId = att.AttachmentId, Size = 5 * Mb } });

            Verify(att).Should().NotContain(m => m.Code == "WB0213");
        }

        [Test]
        public void Size_of_attachment_not_in_questionnaire_should_not_warn()
        {
            attachmentService.Setup(a => a.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(new List<AttachmentSize> { new AttachmentSize { AttachmentId = Guid.NewGuid(), Size = 6 * Mb } });

            Verify(Att("ok")).Should().NotContain(m => m.Code == "WB0213");
        }

        [Test]
        public void Total_size_over_50mb_should_produce_warning_WB0214()
        {
            attachmentService.Setup(a => a.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(new List<AttachmentSize>
                {
                    new AttachmentSize { AttachmentId = Guid.NewGuid(), Size = 30 * Mb },
                    new AttachmentSize { AttachmentId = Guid.NewGuid(), Size = 21 * Mb }
                });

            Verify(Att("ok")).Should().Contain(m => m.Code == "WB0214");
        }

        [Test]
        public void Total_size_of_50mb_should_not_warn()
        {
            attachmentService.Setup(a => a.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(new List<AttachmentSize> { new AttachmentSize { AttachmentId = Guid.NewGuid(), Size = 50 * Mb } });

            Verify(Att("ok")).Should().NotContain(m => m.Code == "WB0214");
        }
    }
}

