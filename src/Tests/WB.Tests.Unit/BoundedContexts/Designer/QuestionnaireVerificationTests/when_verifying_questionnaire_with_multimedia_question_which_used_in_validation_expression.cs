using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_multimedia_question_which_used_in_validation_expression : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var validationExpression = "[var]==1";
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(validationExpression) == new[] { "var" });

            questionnaire = CreateQuestionnaireDocument(new MultimediaQuestion()
            {
                PublicKey = multimediaQuestionId,
                StataExportCaption = "var"
            },
                new TextQuestion()
                {
                    PublicKey = questionWhichUsesMultimediaInConditionExpression,
                    ValidationExpression = validationExpression,
                    ValidationMessage = "message",
                    StataExportCaption = "var1"
                });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            verificationMessages = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0080 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0080");

        It should_return_message_with_2_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_first_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_reference_with_id_of_questionWhichUsesMultimediaInConditionExpression = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionWhichUsesMultimediaInConditionExpression);

        It should_return_second_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_reference_with_id_of_multimediaQuestionId = () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(multimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multimediaQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid questionWhichUsesMultimediaInConditionExpression = Guid.Parse("20000000000000000000000000000000");
    }
}
