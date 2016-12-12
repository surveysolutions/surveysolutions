using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_question_with_empty_and_too_long_validation_messages_in_translation : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                translations: new[]
                {
                    Create.Translation(name: "normal name")
                },
                children: new IComposite[]
                {
                    Create.TextQuestion(validationConditions: new[]
                    {
                        Create.ValidationCondition(expression: "Expression", message: "normal message"),
                    }),

                });
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
                {
                    Create.Translation(name: "normal name")
                },
                children: new IComposite[]
                {
                    Create.TextQuestion(validationConditions: new[]
                    {
                        Create.ValidationCondition(expression: "Expression", message: "Message very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long"),
                    }),

                });
            var questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);

            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        It should_return_messages_with_codes__WB0105 = () =>
            verificationMessages.Select(message => message.Code).ShouldContain("WB0105");

        It should_return_messages_with_translation_name = () =>
            verificationMessages.Single(message => message.Code == "WB0105").Message.ShouldContain("normal name");


        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static List<QuestionnaireVerificationMessage> verificationMessages;
    }
}