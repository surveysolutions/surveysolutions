using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_unused_attachment : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                attachments: new [] { Create.Attachment() }, 
                children : Create.Question(questionType: QuestionType.DateTime));

            attachmentServiceMock = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey, Create.AttachmentView(id: attachment1Id));

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock);
        };

        Because of = () => verificationMessages = verifier.Verify(questionnaire);

        It should_return_WB0215_warning = () =>
            verificationMessages.ShouldContainWarning("WB0215");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IAttachmentService attachmentServiceMock;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
    }
}