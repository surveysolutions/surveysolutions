using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("KP-6779 - ignored before fix by Slava")]
    internal class when_questionnaire_has_attachment_with_size_more_than_5Mb : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Question(questionType: QuestionType.DateTime));
            questionnaire.Attachments.Add(Create.Attachment(attachment1Id, "a"));

            var attachments = new List<AttachmentView>
            {
                Create.AttachmentView(id: attachment1Id, size: 6 * 1024 * 1024)
            };

            attachmentServiceMock
                .Setup(x => x.GetAttachmentsForQuestionnaire(questionnaire.PublicKey))
                .Returns(attachments);

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock.Object);
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0213_warning = () =>
            errors.ShouldContainWarning("WB0213");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static readonly Mock<IAttachmentService> attachmentServiceMock = new Mock<IAttachmentService>();

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
    }
}