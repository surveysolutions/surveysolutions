using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.Conditions
{
    internal class when_verifying_questionnaire_with_roster_that_has_custom_condition_referencing_question_in_other_roster_with_same_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireWithTwoRosterWithConditionInLastOneRosterReferencingQuestionFromFirstOne(
                questionIdFromOtherRosterWithSameLevel,
                rosterWithCustomCondition);

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new [] { questionIdFromOtherRosterWithSameLevel.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.CheckForErrors(questionnaire);

        It should_return_0_error = () =>
            resultErrors.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterWithCustomCondition = Guid.Parse("10000000000000000000000000000000");
        private static Guid questionIdFromOtherRosterWithSameLevel = Guid.Parse("12222222222222222222222222222222");
    }
}