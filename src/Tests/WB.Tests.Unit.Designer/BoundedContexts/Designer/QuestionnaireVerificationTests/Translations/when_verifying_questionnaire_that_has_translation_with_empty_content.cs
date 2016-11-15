using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.Questionnaire.Translations;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_translation_with_empty_content : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "normal name")
            });
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(translations: new[]
            {
                Create.Translation(name: "normal name")
            });

            questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);

            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
        };

        Because of = () => verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));


        It should_return_WB0257_error = () => 
            verificationMessages.ShouldContainError("WB0257");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IQuestionnaireTranslator questionnaireTranslator;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}