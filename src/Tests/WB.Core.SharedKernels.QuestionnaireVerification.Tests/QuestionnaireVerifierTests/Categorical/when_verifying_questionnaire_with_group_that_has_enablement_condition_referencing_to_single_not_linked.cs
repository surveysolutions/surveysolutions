using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests.QRBarcode
{
    internal class when_verifying_questionnaire_with_group_that_has_enablement_condition_referencing_to_categirocal_single_not_linked_question : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                new SingleQuestion()
                {
                    PublicKey = categoricalQuestionId,
                    StataExportCaption = categoricalQuestionId.ToString(),
                    Answers =
                        new List<Answer>()
                        {
                            new Answer() {AnswerValue = "1", AnswerText = "opt 1"},
                            new Answer() {AnswerValue = "2", AnswerText = "opt 2"}
                        }
                },
                new Group()
                {
                    PublicKey = groupWithEnablementConditionId,
                    ConditionExpression = "some condition",
                });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.IsSyntaxValid(it.IsAny<string>()) == true
                   &&
                   processor.GetIdentifiersUsedInExpression(it.IsAny<string>()) ==
                   new[] {categoricalQuestionId.ToString()});

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_have_no_errors = () =>
            resultErrors.ShouldBeEmpty();

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid groupWithEnablementConditionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid categoricalQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}