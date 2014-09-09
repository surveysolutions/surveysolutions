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
    class when_verifying_questionnaire_with_question_that_has_custom_validation_referencing_TextList_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithCustomValidation = Guid.Parse("10000000000000000000000000000000");
            multiAnswerQuestionId = Guid.Parse("12222222222222222222222222222222");

            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new TextListQuestion("Text") { PublicKey = multiAnswerQuestionId, StataExportCaption = "var1" });
            questionnaire.Children.Add(new NumericQuestion
            {
                PublicKey = questionWithCustomValidation,
                StataExportCaption = "var2",
                IsInteger = true,
                MaxValue = 5,
                ValidationExpression = "some validation",
                ValidationMessage = "some message"
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.IsSyntaxValid(Moq.It.IsAny<string>())).Returns(true);

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new string[] { multiAnswerQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0043__ = () =>
          resultErrors.Single().Code.ShouldEqual("WB0043");

        It should_return_error_with_one_reference = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_questionWithCustomValidation = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithCustomValidation);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithCustomValidation;
        private static Guid multiAnswerQuestionId;
    }
}
