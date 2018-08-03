using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_checking_for_errors_questionnaire_that_has_attachment_with_empty_content : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionId, 
                attachments: new[] { Create.Attachment(attachment1Id) });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0111 () =>
            verificationMessages.ShouldContainError("WB0111");

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.GetError("WB0111").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Attachment () =>
            verificationMessages.GetError("WB0111").References.Should().OnlyContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Attachment);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_attachment1Id () =>
            verificationMessages.GetError("WB0111").References.Single().Id.Should().Be(attachment1Id);


        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
    }
}