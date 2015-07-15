using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.CustomEnablementConditions
{
    internal class when_verifying_questionnaire_with_roster_inside_roster_and_second_one_has_custom_condition_referencing_question_inside_itself : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterQuestionId = Guid.Parse("a3333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion
                {
                    PublicKey = rosterQuestionId,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new Group
                {
                    PublicKey = Guid.Parse("aa333333333333333333333333333333"),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group
                        {
                            PublicKey = groupWithCustomCondition,
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterQuestionId,
                            ConditionExpression = "some random expression",
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    StataExportCaption = "var2",
                                    PublicKey = underDeeperRosterLevelQuestionId
                                }
                            }
                        }
                    }
                }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new string[] { underDeeperRosterLevelQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0046 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0051");

        It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Group = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_reference_with_id_of_groupWithCustomCondition = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(groupWithCustomCondition);

        It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_error_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid groupWithCustomCondition = Guid.Parse("10000000000000000000000000000000");
        private static Guid underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}