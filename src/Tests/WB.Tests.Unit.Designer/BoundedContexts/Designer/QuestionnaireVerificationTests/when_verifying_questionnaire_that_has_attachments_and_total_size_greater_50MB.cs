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
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(attachments: new[] { Create.Attachment(), Create.Attachment() });

            attachmentServiceMock = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey, 
                Create.AttachmentView(size: 26 * 1024 * 1024),
                Create.AttachmentView(size: 26 * 1024 * 1024));

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock);
        };

        Because of = () => verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_WB0214_warning = () => 
            verificationMessages.ShouldContainWarning("WB0214");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IAttachmentService attachmentServiceMock;
    }
}