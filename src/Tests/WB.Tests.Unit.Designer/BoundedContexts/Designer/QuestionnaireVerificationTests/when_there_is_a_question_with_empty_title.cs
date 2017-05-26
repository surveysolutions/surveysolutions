using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_question_has_empty_title : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(Create.TextQuestion(text: ""));
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_WB0269_message = () => verificationMessages.ShouldContainError("WB0269");

        It should_return_WB0269_message_with_appropriate_message = () =>
            verificationMessages.ShouldContain(x => x.Message == VerificationMessages.WB0269_QuestionTitleIsEmpty);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}