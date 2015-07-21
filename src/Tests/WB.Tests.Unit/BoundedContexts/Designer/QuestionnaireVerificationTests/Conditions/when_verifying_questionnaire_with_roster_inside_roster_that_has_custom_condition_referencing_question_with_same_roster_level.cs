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
    internal class when_verifying_questionnaire_with_roster_inside_roster_that_has_custom_condition_referencing_question_with_same_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestion = Guid.Parse("a3333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion
                {
                    PublicKey = rosterSizeQuestion,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new Group
                {
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestion,
                    Children = new List<IComposite>
                    {
                        new Group
                        {
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeQuestion,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    StataExportCaption = "var2",
                                    PublicKey = questionIdFromOtherRosterWithSameLevel
                                }
                            }
                        },
                        new Group
                        {
                            IsRoster = true,
                            VariableName = "c",
                            RosterSizeQuestionId = rosterSizeQuestion,
                            PublicKey = rosterWithCustomCondition,
                            ConditionExpression = "some random expression"
                        }
                    }
                }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new [] { questionIdFromOtherRosterWithSameLevel.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_0_error = () =>
            resultErrors.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterWithCustomCondition = Guid.Parse("10000000000000000000000000000000");
        private static Guid questionIdFromOtherRosterWithSameLevel = Guid.Parse("12222222222222222222222222222222");
    }
}