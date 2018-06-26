using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_enablement_conditions_ : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            const string incorrectConditionExpression = "[hehe] &=+< 5";
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion
                {
                    PublicKey = questionId,
                    ConditionExpression = incorrectConditionExpression,
                    StataExportCaption = "var1"
                }
            );

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error_with_code__WB0003__ () =>
            verificationMessages.GetError("WB0003").Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_error_with_single_reference () =>
            verificationMessages.GetError("WB0003").References.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_error_referencing_with_type_of_question () =>
            verificationMessages.GetError("WB0003").References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_error_referencing_with_specified_question_id () =>
            verificationMessages.GetError("WB0003").References.Single().Id.Should().Be(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}