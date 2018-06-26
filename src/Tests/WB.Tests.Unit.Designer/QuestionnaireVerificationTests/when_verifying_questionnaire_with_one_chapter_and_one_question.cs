using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_one_chapter_and_one_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.TextQuestion(variable: "var", text: "test")
            );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_no_errors () =>
            verificationMessages.Should().BeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IQuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}
