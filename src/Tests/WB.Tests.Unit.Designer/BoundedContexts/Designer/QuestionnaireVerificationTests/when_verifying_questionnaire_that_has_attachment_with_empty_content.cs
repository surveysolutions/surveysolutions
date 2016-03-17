using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("KP-6779 - ignored before fix by Slava")]
    class when_verifying_questionnaire_that_has_attachment_with_empty_content : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionId, Create.TextQuestion(variable: "var"));
            questionnaire.Attachments.Add(Create.Attachment(attachment1Id, "hello"));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0108 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0108");

        It should_return_message_with_General_level = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_1_reference = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Attachment = () =>
            verificationMessages.Single().References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Attachment);

        It should_return_message_reference_with_id_of_attachment1Id = () =>
            verificationMessages.Single().References.Single().Id.ShouldEqual(attachment1Id);


        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
    }
}