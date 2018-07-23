using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_attachments_and_total_size_greater_50MB : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(attachments: new[] { Create.Attachment(), Create.Attachment() });

            attachmentServiceMock = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey, 
                Create.AttachmentView(size: 26 * 1024 * 1024),
                Create.AttachmentView(size: 26 * 1024 * 1024));

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock);
            BecauseOf();
        }

        private void BecauseOf() => verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0214_warning () => 
            verificationMessages.ShouldContainWarning("WB0214");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IAttachmentService attachmentServiceMock;
    }
}
