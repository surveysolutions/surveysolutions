using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_checking_for_errors_questionnaire_that_has_attachment_with_empty_content : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionId, 
                attachments: new[] { Create.Attachment(attachment1Id) });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_message_with_code__WB0111 = () =>
            verificationMessages.ShouldContainError("WB0111");

        It should_return_message_with_1_reference = () =>
            verificationMessages.GetError("WB0111").References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Attachment = () =>
            verificationMessages.GetError("WB0111").References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Attachment);

        It should_return_message_reference_with_id_of_attachment1Id = () =>
            verificationMessages.GetError("WB0111").References.Single().Id.ShouldEqual(attachment1Id);


        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
    }
}