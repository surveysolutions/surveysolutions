using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.Questionnaire.Translations;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Translations
{
    internal class when_verifying_questionnaire_that_has_translation_with_invalid_name : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "invalid 7 _@ # ; ' /? @@ name invalid invalid invalid invalid invalid")
            });
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "invalid 7 _@ # ; ' /? @@ name invalid invalid invalid invalid invalid")
            });

            questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);

            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
            BecauseOf();
        }

        private void BecauseOf() => verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0256_error () => 
            verificationMessages.ShouldContainError("WB0256");


        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IQuestionnaireTranslator questionnaireTranslator;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}