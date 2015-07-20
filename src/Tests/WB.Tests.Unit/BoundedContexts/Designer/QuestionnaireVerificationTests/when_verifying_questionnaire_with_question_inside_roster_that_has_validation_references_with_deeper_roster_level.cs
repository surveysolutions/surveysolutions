using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_inside_roster_that_has_validation_references_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.NumericQuestion(questionId: rosterSizeQuestionId, isInteger: true, variableName: "var1"),
                Create.Roster(rosterId: rosterGroupId, variable: "a", children: new IComposite[]
                {
                    Create.TextQuestion(questionId: questionWithConditionId, variable: "var2",
                        validationExpression: String.Format("{0}==2", underDeeperRosterLevelQuestionVariableName), validationMessage:"message"),
                    Create.Roster(rosterId: rosterGroupId, variable: "c",
                        children:
                            new IComposite[]
                            {
                                Create.NumericIntegerQuestion(id: underDeeperRosterLevelQuestionId,
                                    variable: underDeeperRosterLevelQuestionVariableName)
                            })
                })
            });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
              => processor.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()) ==
                 new[] { underDeeperRosterLevelQuestionVariableName });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0014 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0014");

        It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithConditionId);

        It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_error_reference_with_id_of_underDeeperPropagationLevelQuestionVariableName = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithConditionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}