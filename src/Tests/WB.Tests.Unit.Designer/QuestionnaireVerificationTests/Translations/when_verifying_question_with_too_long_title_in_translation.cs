using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Translations
{
    internal class when_verifying_question_with_too_long_title_in_translation : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                translations: new[] { Create.Translation(name: "normal name") },
                children: new IComposite[] { Create.TextQuestion(text: "normal title"), });
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(
                translations: new[] { Create.Translation(name: "normal name") },
                children: new IComposite[] { Create.TextQuestion(text: "long title" + new string('l', 500)), });

            var questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);
            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_messages_with_codes__WB0259 () =>
            verificationMessages.Select(message => message.Code).Should().Contain("WB0259");

        [NUnit.Framework.Test] public void should_return_messages_with_translation_name () =>
            verificationMessages.Single(message => message.Code == "WB0259").Message.Should().Contain("normal name");


        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static List<QuestionnaireVerificationMessage> verificationMessages;
    }
}
