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
    internal class when_verifying_questionnaire_that_has_attachment_with_size_more_than_5Mb : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(attachments: new[] { Create.Attachment() });

            attachmentServiceMock = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey,
                Create.AttachmentView(size: 6*1024*1024));

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock);
        };

        Because of = () => verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_WB0213_warning = () =>
            verificationMessages.ShouldContainWarning("WB0213");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IAttachmentService attachmentServiceMock;
    }
}