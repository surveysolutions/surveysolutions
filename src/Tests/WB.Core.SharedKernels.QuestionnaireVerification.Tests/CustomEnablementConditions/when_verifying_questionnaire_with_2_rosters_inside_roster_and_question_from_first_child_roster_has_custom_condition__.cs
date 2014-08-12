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
    [Ignore("C#")]
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_question_from_first_child_roster_has_custom_condition_referencing_question_from_second_child_roster_and_all_roster_size_questions_is_different : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var rosterGroupId =    Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterQuestionId =   Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterChildGroup1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterChildGroup2Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var rosterInsideRosterQuestion1Id =   Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var rosterInsideRosterQuestion2Id =   Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
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
                    RosterSizeQuestionId = rosterQuestionId,
                    VariableName = "a",
                    Children = new List<IComposite>
                    {
                        new NumericQuestion
                        {
                            PublicKey = rosterInsideRosterQuestion1Id,
                            StataExportCaption = rosterInsideRosterQuestion1Id.ToString(),
                            IsInteger = true,
                            MaxValue = 5
                        },
                        new NumericQuestion
                        {
                            PublicKey = rosterInsideRosterQuestion2Id,
                            StataExportCaption = rosterInsideRosterQuestion2Id.ToString(),
                            IsInteger = true,
                            MaxValue = 5
                        },
                        new Group
                        {
                            PublicKey = rosterChildGroup1Id,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterInsideRosterQuestion1Id,
                            VariableName = "b",
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    StataExportCaption = underDeeperRosterLevelQuestionId.ToString(),
                                    PublicKey = underDeeperRosterLevelQuestionId
                                }
                            }
                        },
                        new Group
                        {
                            PublicKey = rosterChildGroup2Id,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterInsideRosterQuestion2Id,
                            VariableName = "c",
                            Children = new List<IComposite>
                            {
                                new SingleQuestion
                                {
                                    PublicKey = questionWithCustomCondition,
                                    StataExportCaption = questionWithCustomCondition.ToString(),
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