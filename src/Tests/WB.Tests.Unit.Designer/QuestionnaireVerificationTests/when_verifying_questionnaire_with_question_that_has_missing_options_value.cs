using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_that_has_missing_options_value : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionWithMissingValues = Guid.Parse("10000000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion(
            
                 questionWithMissingValues,
                variable: "var",
                answers: new List<Answer> {
                    new Answer() {                    AnswerText = "opt 1" },
                    new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0045 () =>
            verificationMessages.First().Code.Should().Be("WB0045");

        [NUnit.Framework.Test] public void should_return_message_with_one_reference () =>
            verificationMessages.First().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.First().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_questionWithCustomCondition () =>
            verificationMessages.First().References.First().Id.Should().Be(questionWithMissingValues);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithMissingValues;
    }
}
