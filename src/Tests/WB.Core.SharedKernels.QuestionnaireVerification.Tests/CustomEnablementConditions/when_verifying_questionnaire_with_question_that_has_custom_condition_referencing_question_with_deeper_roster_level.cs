using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.CustomEnablementConditions
{
    internal class when_verifying_questionnaire_with_question_that_has_custom_condition_referencing_question_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireWithRosterAndQuestionAfterWithConditionReferencingQuestionInRoster(
                underDeeperRosterLevelQuestionId,
                questionWithCustomCondition);

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor
                .Setup(x => x.IsSyntaxValid(Moq.It.IsAny<string>()))
                .Returns(true);

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new[] { underDeeperRosterLevelQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0046 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0046");

        It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_questionWithCustomCondition = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithCustomCondition);

        It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_error_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid questionWithCustomCondition = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}