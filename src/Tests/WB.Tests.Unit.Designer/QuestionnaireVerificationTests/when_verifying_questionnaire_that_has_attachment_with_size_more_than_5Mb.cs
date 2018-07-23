using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_attachment_with_size_more_than_5Mb : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid attachmentId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(attachments: new[] { Create.Attachment(attachmentId: attachmentId) });

            attachmentServiceMock = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey,
                Create.AttachmentView(id: attachmentId, size: 6*1024*1024));

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock);
            BecauseOf();
        }

        private void BecauseOf() => verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0213_warning () =>
            verificationMessages.ShouldContainWarning("WB0213");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IAttachmentService attachmentServiceMock;
    }
}