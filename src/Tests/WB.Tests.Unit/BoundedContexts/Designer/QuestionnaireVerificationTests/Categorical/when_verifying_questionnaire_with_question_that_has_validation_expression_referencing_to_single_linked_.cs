using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_question_that_has_validation_expression_referencing_to_categorical_single_linked_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var variableOfLinkedQuestion = "linked";

            questionnaire = Create.QuestionnaireDocument(id: Id.g1, children: Create.Group(groupId: Id.g2, children: new List<IComposite>
            {
                Create.Roster(rosterId: Id.g3, 
                    rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                    variable: "a",
                    fixedRosterTitles: new[] { Create.FixedRosterTitle(1, "fixed title 1"), Create.FixedRosterTitle(3, "fixed title 3") },
                    children: new List<IComposite>
                    {
                        Create.TextQuestion(questionId: LinkedSourceQuestionId, variable: "v")
                    }),
                Create.SingleQuestion(id: categoricalQuestionId, variable: variableOfLinkedQuestion, linkedToQuestionId: LinkedSourceQuestionId),
                Create.NumericIntegerQuestion(
                    id: questionWithValidationExpressionId, 
                    variable: "v2",
                    validationConditions: new List<ValidationCondition> { Create.ValidationCondition(validationWithLinkedQuestion, "some message") })
            }));

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(validationWithLinkedQuestion) == new[] { variableOfLinkedQuestion });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0063__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0063");

        It should_return_message_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_question_with_validation_expression = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithValidationExpressionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid LinkedSourceQuestionId = Id.g4;
        private static readonly Guid categoricalQuestionId = Id.g5;
        private static readonly Guid questionWithValidationExpressionId = Id.g6;
        private static readonly string validationWithLinkedQuestion = "some validation";
        
    }
}