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
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_question_from_first_child_roster_has_custom_condition_referencing_question_from_second_child_roster_and_questions_have_the_same_roster_levels : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var questionWithCustomCondition = Guid.Parse("10000000000000000000000000000000");
            var underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterChildGroup1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterChildGroup2Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var rosterInsideRosterQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

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
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion
                        {
                            PublicKey = rosterInsideRosterQuestionId,
                            StataExportCaption = "var2",
                            IsInteger = true
                        },
                        new Group
                        {
                            PublicKey = rosterChildGroup1Id,
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterInsideRosterQuestionId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    StataExportCaption = "var3",
                                    PublicKey = underDeeperRosterLevelQuestionId
                                }
                            }
                        },
                        new Group
                        {
                            PublicKey = rosterChildGroup2Id,
                            IsRoster = true,
                            VariableName = "c",
                            RosterSizeQuestionId = rosterInsideRosterQuestionId,
                            Children = new List<IComposite>
                            {
                                new SingleQuestion
                                {
                                    PublicKey = questionWithCustomCondition,
                                    StataExportCaption = "var4",
                                    ConditionExpression = "some random expression",
                                    Answers = { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
                                }
                            }
                        }

                    }
                }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new[] { underDeeperRosterLevelQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_0_errors = () =>
             resultErrors.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}