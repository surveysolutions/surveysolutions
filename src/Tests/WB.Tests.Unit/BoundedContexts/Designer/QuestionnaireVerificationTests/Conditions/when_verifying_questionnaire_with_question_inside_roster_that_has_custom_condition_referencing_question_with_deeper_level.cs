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
    [Ignore("KP-4518")]
    internal class when_verifying_questionnaire_with_question_inside_roster_that_has_custom_condition_referencing_question_with_deeper_roster_level_and_rosters_have_the_same_roster_size_questions : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var rosterGroupId =    Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterQuestionId =   Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterChildGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
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
                        new Group
                        {
                            PublicKey = rosterChildGroupId,
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterQuestionId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    StataExportCaption = "var2",
                                    PublicKey = underDeeperRosterLevelQuestionId
                                }
                            }
                        },
                        new SingleQuestion
                        {
                            StataExportCaption = "var3",
                            PublicKey = questionWithCustomCondition,
                            ConditionExpression = "some random expression",
                            Answers = { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
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