using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_linked_question_with_incorrect_syntax_in_filter_expression : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId, fixedTitles: new[] { "1", "2", "3" },children: new[]
                {
                    Create.NumericIntegerQuestion(variable: "enumeration_district"),
                }),

                Create.Group(groupId: groupId, children: new[]
                {
                    Create.SingleOptionQuestion(questionId: questionId, 
                    variable: "s546i",
                    linkedToRosterId: rosterId,
                    linkedFilterExpression: "incorrect [] expression")
                })
            });

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0110 () =>
            verificationMessages.First().Code.Should().Be("WB0110");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            verificationMessages.First().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_one_references_with_question_type () =>
            verificationMessages.First().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_one_references_with_id_equals_questionId () =>
            verificationMessages.First().References.First().Id.Should().Be(questionId);
        
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

    }
}