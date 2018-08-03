using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_validation_and_condition : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            const string invalidExpression = "[hehe] &=+< 5";
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.TextQuestion(
                    questionId,
                    validationExpression: invalidExpression,
                    enablementCondition: invalidExpression, 
                    validationMessage: "validation message",
                    variable: "var1"
                ));

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_2_messages () =>
            verificationMessages.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0003__ () =>
            verificationMessages.Select(error => error.Code).Should().Contain("WB0003");

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0002__ () =>
            verificationMessages.Select(error => error.Code).Should().Contain("WB0002");

        [NUnit.Framework.Test] public void should_return_message_with_single_reference () =>
            verificationMessages.Should().OnlyContain(error=>error.References.Count() == 1);

        [NUnit.Framework.Test] public void should_return_message_referencing_with_type_of_question () =>
            verificationMessages.Should().OnlyContain(error=>error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_referencing_with_specified_question_id () =>
            verificationMessages.Should().OnlyContain(error=>error.References.Single().Id == questionId);

        private static List<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}