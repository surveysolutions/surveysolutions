using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.CustomEnablementConditions
{
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_first_child_roster_has_custom_condition_referencing_question_from_second_child_roster : QuestionnaireVerifierTestsContext
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
                    StataExportCaption = rosterQuestionId.ToString(),
                    IsInteger = true,
                    MaxValue = 5
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
                            StataExportCaption = rosterInsideRosterQuestionId.ToString(),
                            IsInteger = true,
                            MaxValue = 5
                        },
                        new Group
                        {
                            PublicKey = rosterChildGroup1Id,
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterInsideRosterQuestionId,
                            ConditionExpression = "some random expression",
                            Children = new List<IComposite>
                            {
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
                                    StataExportCaption = underDeeperRosterLevelQuestionId.ToString(),
                                    PublicKey = underDeeperRosterLevelQuestionId,
                                    Answers = { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
                                }
                            }
                        }

                    }
                }
            });

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

        It should_return_0_errors = () =>
             resultErrors.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}