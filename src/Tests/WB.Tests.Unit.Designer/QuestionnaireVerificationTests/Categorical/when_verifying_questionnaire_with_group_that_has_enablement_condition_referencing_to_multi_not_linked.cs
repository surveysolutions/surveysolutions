using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_group_that_has_enablement_condition_referencing_to_categirocal_multi_not_linked_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.MultyOptionsQuestion(
                    categoricalQuestionId,
                    variable: "test",
                    options:
                        new List<Answer>()
                        {
                            new Answer() {AnswerValue = "1", AnswerText = "opt 1"},
                            new Answer() {AnswerValue = "2", AnswerText = "opt 2"}
                        }
                ),
                new Group()
                {
                    Title = "Title",
                    PublicKey = groupWithEnablementConditionId,
                    ConditionExpression = "some condition",
                });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()) == new[] { categoricalQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
            BecauseOf();
        }

        private void BecauseOf() =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_have_no_errors () =>
           resultErrors.Should().BeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid groupWithEnablementConditionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid categoricalQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}
