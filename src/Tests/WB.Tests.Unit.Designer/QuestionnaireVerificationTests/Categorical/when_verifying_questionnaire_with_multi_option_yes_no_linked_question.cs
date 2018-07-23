using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_multi_option_yes_no_linked_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.Roster(rosterId: Guid.NewGuid(), variable:"ros", children: new[]
                {
                    Create.TextQuestion(questionId: linkedQuestionId, variable: "var2")
                }),
                Create.MultyOptionsQuestion(id: multiOptionQuestionId, variable: "var", linkedToQuestionId: linkedQuestionId, yesNoView: true));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error_with_code__WB0007__ () =>
            resultErrors.ShouldContainError("WB0007");

        [NUnit.Framework.Test] public void should_return_error__WB0007__with_one_references () =>
            resultErrors.GetError("WB0007").References.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_error__WB0007__reference_with_type_Question () =>
            resultErrors.GetError("WB0007").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_error__WB0007__reference_with_id_of_multi_option_question () =>
            resultErrors.GetError("WB0007").References.First().Id.Should().Be(multiOptionQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid multiOptionQuestionId = Guid.Parse("12222222222222222222222222222222");
        private static Guid linkedQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}