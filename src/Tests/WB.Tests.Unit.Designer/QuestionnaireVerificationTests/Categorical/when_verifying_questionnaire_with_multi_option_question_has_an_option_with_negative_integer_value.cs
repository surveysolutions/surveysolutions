using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_multi_option_question_has_an_option_with_negative_integer_value : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.MultyOptionsQuestion(id: multiOptionQuestionId, variable: "var",
                    options: new[]
                    {
                        Create.Answer("-13", -13m),
                        Create.Answer("-2", -2)
                    }));


            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_have_0_error () =>
            resultErrors.Count().Should().Be(0);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid multiOptionQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}