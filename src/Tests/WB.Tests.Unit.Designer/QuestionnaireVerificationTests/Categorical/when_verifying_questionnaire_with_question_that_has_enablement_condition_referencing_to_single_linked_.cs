using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_question_that_has_enablement_condition_referencing_to_categorical_single_linked_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var linkedSourceQuestionId = Guid.Parse("33333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(Create.FixedRoster(variable: "a",
                fixedTitles: new[] {"fixed title 1", "fixed title 2"},
                children: new IComposite[]
                {
                    new TextQuestion("test")
                    {
                        PublicKey = linkedSourceQuestionId,
                        QuestionType = QuestionType.Text,
                        StataExportCaption = "var"
                    }
                }),
                new SingleQuestion("test")
                {
                    PublicKey = categoricalQuestionId,
                    StataExportCaption = "var1",
                    LinkedToQuestionId = linkedSourceQuestionId
                },
                new NumericQuestion("test")
                {
                    PublicKey = questionWithEnablementConditionId,
                    ConditionExpression = "some condition",
                    StataExportCaption = "var2"
                });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()) == new[] { categoricalQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
            BecauseOf();
        }

        private void BecauseOf() =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_no_errors () =>
            resultErrors.Count().Should().Be(0);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithEnablementConditionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid categoricalQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}