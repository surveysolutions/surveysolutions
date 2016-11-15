using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_title_has_invalid_characters : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Title = "this is title [variable]";
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_WB0097_message = () => verificationMessages.ShouldContain(x => x.Code == "WB0097");

        It should_return_WB0097_message_with_appropriate_message = () =>
            verificationMessages.ShouldContain(x => x.Message == VerificationMessages.WB0097_QuestionnaireTitleHasInvalidCharacters);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}

