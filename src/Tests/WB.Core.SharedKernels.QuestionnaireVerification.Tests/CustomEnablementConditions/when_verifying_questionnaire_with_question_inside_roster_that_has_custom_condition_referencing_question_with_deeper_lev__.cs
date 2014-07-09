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
    internal class when_verifying_questionnaire_with_question_inside_roster_that_has_custom_condition_referencing_question_with_deeper_roster_level_and_rosters_have_different_roster_size_questions : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var rosterGroupId =    Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterQuestionId =   Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterChildGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterInsideRosterQuestionId =   Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
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
                            PublicKey = rosterInsideRosterQuestionId,
                            StataExportCaption = rosterInsideRosterQuestionId.ToString(),
                            IsInteger = true,
                            MaxValue = 5
                        },
                        new Group
                        {
                            PublicKey = rosterChildGroupId,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterInsideRosterQuestionId,
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
                        new SingleQuestion
                        {
                            StataExportCaption = questionWithCustomCondition.ToString(),
                            PublicKey = questionWithCustomCondition,
                            ConditionExpression = "some random expression",
                            Answers = { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
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