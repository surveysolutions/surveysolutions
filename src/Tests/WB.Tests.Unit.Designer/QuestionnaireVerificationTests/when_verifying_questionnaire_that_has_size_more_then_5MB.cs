using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_size_more_then_5MB : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                new TextQuestion(new string('q', 5 * 1024 * 1024))
                {
                    StataExportCaption = "var0"
                }); 
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        
        [NUnit.Framework.Test] public void should_return_message_with_code__WB0098 () =>
            verificationMessages.ShouldContainError("WB0098");

        [NUnit.Framework.Test] public void should_return_WB0098_error_with_appropriate_message () =>
            verificationMessages.Single(m => m.Code == "WB0098").Message.Should().NotBeEmpty();
        


        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}
