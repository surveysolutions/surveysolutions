using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_multimedia_question_which_used_in_groups_condition_expression : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var conditionExpression = "[var]==1";
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                =>
                processor.IsSyntaxValid(conditionExpression) == true &&
                    processor.GetIdentifiersUsedInExpression(conditionExpression) == new[] { "var" });

            questionnaire = CreateQuestionnaireDocument(new MultimediaQuestion()
            {
                PublicKey = multimediaQuestionId,
                StataExportCaption = "var"
            },
                new Group()
                {
                    PublicKey = groupWhichUsesMultimediaInConditionExpression,
                    ConditionExpression = conditionExpression,
                    VariableName = "var1"
                });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0081 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0081");

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_reference_with_id_of_groupWhichUsesMultimediaInConditionExpression = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(groupWhichUsesMultimediaInConditionExpression);

        It should_return_second_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_multimediaQuestionId = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(multimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multimediaQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid groupWhichUsesMultimediaInConditionExpression = Guid.Parse("20000000000000000000000000000000");
    }
}
