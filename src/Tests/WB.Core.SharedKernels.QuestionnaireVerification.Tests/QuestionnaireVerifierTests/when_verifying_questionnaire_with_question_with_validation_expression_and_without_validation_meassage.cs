using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_question_with_validation_expression_and_without_validation_meassage : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = questionId,
                    ValidationExpression = validationExpression,
                    StataExportCaption = "var1"
                });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.IsSyntaxValid(validationExpression) == true);

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_errors_each_with_code__WB0002__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0065");

        It should_return_errors_each_having_single_reference = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_errors_each_referencing_question = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_referencing_first_incorrect_question = () =>
            resultErrors.Single().References.Single().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId;
        private const string validationExpression = "some expression";
    }
}