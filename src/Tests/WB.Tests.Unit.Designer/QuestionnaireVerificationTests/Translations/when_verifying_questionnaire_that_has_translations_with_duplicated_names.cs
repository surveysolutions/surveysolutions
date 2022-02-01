using System.Collections.Generic;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.Questionnaire.Translations;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Translations
{
    internal class should_not_allow_duplicate_original_and_translation_name : QuestionnaireVerifierTestsContext
    {
    
        [Test]
        public void should_return_WB0258_error()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "duplicated name"),
            });
            questionnaire.DefaultLanguageName = "duplicated name";
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "duplicated name"),
            });

            var questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);

            var verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
            
            var errors = verifier.CompileAndVerify(Create.QuestionnaireView(questionnaire), null, out string _);
            
            errors.ShouldContainError("WB0258");
        }    
    }
    
    internal class when_verifying_questionnaire_that_has_translations_with_duplicated_names : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "duplicated name"),
                Create.Translation(name: "duplicated name  "),
                Create.Translation(name: "unique name")
            });
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "duplicated name"),
                Create.Translation(name: "duplicated name  "),
                Create.Translation(name: "unique name")
            });

            questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);

            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
            BecauseOf();
        }

        private void BecauseOf() => verificationMessages = verifier.CompileAndVerify(Create.QuestionnaireView(questionnaire), null, out string _);

        [NUnit.Framework.Test] public void should_return_WB0258_error () =>
            verificationMessages.ShouldContainError("WB0258");


        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IQuestionnaireTranslator questionnaireTranslator;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}
