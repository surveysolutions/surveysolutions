using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_single_option_question_has_an_option_with_decimal_value : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion(questionId: optionQuestionId, variable: "var",
                    answerCodes: new[] {3.3m, 4m}));
            

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_have_1_error () =>
            resultErrors.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0114__ () =>
        resultErrors.Single().Code.Should().Be("WB0114");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            resultErrors.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            resultErrors.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_multi_option_question () =>
            resultErrors.Single().References.First().Id.Should().Be(optionQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid optionQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}