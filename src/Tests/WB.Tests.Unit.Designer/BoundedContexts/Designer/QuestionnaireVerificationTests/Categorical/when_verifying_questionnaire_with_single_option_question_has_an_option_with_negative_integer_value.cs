using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_single_option_question_has_an_option_with_negative_integer_value : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion(questionId: optionQuestionId, variable: "var",
                    answerCodes: new[] {-3m, -14}));
            

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_have_0_error = () =>
            resultErrors.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid optionQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}