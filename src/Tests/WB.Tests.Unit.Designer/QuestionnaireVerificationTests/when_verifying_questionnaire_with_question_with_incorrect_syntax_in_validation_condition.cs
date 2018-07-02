using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_validation_condition : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.TextQuestion
                (
                     questionId,
                    validationExpression: "[hehe] &=+< 5",
                    validationMessage: "validation message",
                    variable: "var1"
                ));

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0002__ () =>
            verificationMessages.First().Code.Should().Be("WB0002");

        [NUnit.Framework.Test] public void should_return_message_with_single_reference () =>
            verificationMessages.First().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_referencing_with_type_of_question () =>
            verificationMessages.First().References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_referencing_with_specified_question_id () =>
            verificationMessages.First().References.Single().Id.Should().Be(questionId);

        private static List<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}