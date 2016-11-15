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
    [Ignore("reference validation is turned off")]
    internal class when_verifying_questionnaire_with_2_questions_referencing_not_existing_question_by_id_in_validation_expression : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            const string ValidationExpressionWithNotExistingQuestion = "[99999999999999999999999999999999] == 2";

            firstIncorrectQuestionId = Guid.Parse("11111111111111111111111111111111");
            secondIncorrectQuestionId = Guid.Parse("22222222222222222222222222222222");
            textQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = firstIncorrectQuestionId,

                    ValidationMessage = "some message",
                    ValidationExpression = ValidationExpressionWithNotExistingQuestion,
                    StataExportCaption = "var1"
                },
                new NumericQuestion
                {
                    PublicKey = secondIncorrectQuestionId,
                    ValidationMessage = "some message",
                    ValidationExpression = ValidationExpressionWithNotExistingQuestion,
                    StataExportCaption = "var2"
                },
                new TextQuestion { PublicKey = textQuestionId, StataExportCaption = "var3" },
                new Group { PublicKey = Guid.NewGuid() }
            );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(ValidationExpressionWithNotExistingQuestion) == new[] { "99999999999999999999999999999999" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_2_messages = () =>
            verificationMessages.Count().ShouldEqual(2);

        It should_return_messages_each_with_code__WB0004__ = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.Code == "WB0004");

        It should_return_messages_each_having_single_reference = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_messages_each_referencing_question = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_message_referencing_first_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_message_referencing_second_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == secondIncorrectQuestionId);

        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid textQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}