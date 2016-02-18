using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_title_has_invalid_characters : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Title = "this is title [variable]";
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_WB0097_message = () => verificationMessages.ShouldContain(x => x.Code == "WB0097");

        It should_return_WB0097_message_with_appropriate_message = () =>
            verificationMessages.ShouldContain(x => x.Message == "Questionnaire title contains characters that are not allowed. Only letters, numbers, space and _ are allowed.");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}

