using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_attachments_and_total_size_greater_50MB : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                attachments: new[] { Create.Attachment(attachment1Id), Create.Attachment(attachment2Id) },
                children: Create.Question(questionType: QuestionType.DateTime));

            attachmentServiceMock = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey, 
                Create.AttachmentView(size: 26 * 1024 * 1024),
                Create.AttachmentView(size: 26 * 1024 * 1024));

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock);
        };

        Because of = () => verificationMessages = verifier.Verify(questionnaire);

        It should_return_WB0214_warning = () => 
            verificationMessages.ShouldContainWarning("WB0214");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IAttachmentService attachmentServiceMock;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachment2Id = Guid.Parse("22222222222222222222222222222222");
    }
}