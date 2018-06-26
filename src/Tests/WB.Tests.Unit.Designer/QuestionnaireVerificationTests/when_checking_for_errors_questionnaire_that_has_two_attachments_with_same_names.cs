using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_checking_for_errors_questionnaire_that_has_two_attachments_with_same_names : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                attachments: new[] { Create.Attachment(attachment1Id, "hello"), Create.Attachment(attachment2Id, "hello") });

            var attachmentService = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey);

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentService);
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));
       
        [NUnit.Framework.Test] public void should_return_message_with_code__WB0065 () =>
            verificationMessages.ShouldContainError("WB0065");

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.GetError("WB0065").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Attachment () =>
            verificationMessages.GetError("WB0065").References.Should().OnlyContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Attachment);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_attachment1Id () =>
            verificationMessages.GetError("WB0065").References.ElementAt(0).Id.Should().Be(attachment1Id);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_attachment2Id () =>
            verificationMessages.GetError("WB0065").References.ElementAt(1).Id.Should().Be(attachment2Id);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachment2Id = Guid.Parse("22222222222222222222222222222222");
    }
}