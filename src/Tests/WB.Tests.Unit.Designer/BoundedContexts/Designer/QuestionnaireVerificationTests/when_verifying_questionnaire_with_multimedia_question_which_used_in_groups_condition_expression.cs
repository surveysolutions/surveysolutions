using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_multimedia_question_which_used_in_groups_condition_expression : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var conditionExpression = "[var]==1";
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(conditionExpression) == new[] { "var" });

            questionnaire = CreateQuestionnaireDocument(new MultimediaQuestion()
            {
                PublicKey = multimediaQuestionId,
                StataExportCaption = "var"
            },
                new Group()
                {
                    PublicKey = groupWhichUsesMultimediaInConditionExpression,
                    ConditionExpression = conditionExpression,
                    VariableName = "var1"
                });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_level_general = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_code__WB0081 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0081");

        It should_return_message_with_2_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_first_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_message_reference_with_id_of_groupWhichUsesMultimediaInConditionExpression = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(groupWhichUsesMultimediaInConditionExpression);

        It should_return_second_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_reference_with_id_of_multimediaQuestionId = () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(multimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multimediaQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid groupWhichUsesMultimediaInConditionExpression = Guid.Parse("20000000000000000000000000000000");
    }
}
