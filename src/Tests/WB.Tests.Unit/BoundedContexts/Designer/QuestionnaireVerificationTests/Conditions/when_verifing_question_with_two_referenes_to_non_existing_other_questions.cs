using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.Conditions
{
    [Ignore("reference validation is turned off")]
    internal class when_verifing_question_with_two_referenes_to_non_existing_other_questions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            const string conditionExpression = "nonExist1 > 0 && nonExist2 > 1";
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion
                {
                    PublicKey = Guid.Parse("11111111111111111111111111111111"),
                    StataExportCaption = "variable",
                    QuestionText = "text",
                    ConditionExpression = conditionExpression
                },
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression(conditionExpression))
                .Returns(new[] { "nonExist1", "nonExist2" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        private Because of = () => errors = verifier.Verify(questionnaire);

        private It should_not_duplicate_errors = () => errors.Count(x => x.Code == "WB0005").ShouldEqual(1);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> errors;
    }
}

