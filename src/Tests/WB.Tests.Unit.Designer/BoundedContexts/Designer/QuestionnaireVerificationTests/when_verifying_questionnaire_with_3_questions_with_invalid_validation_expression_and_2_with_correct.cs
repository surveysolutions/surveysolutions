using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_3_questions_with_invalid_validation_expression_and_with_2_with_correct : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            const string InvalidExpression = "[hehe] &=+< 5";
            const string ValidExpression = "var1 > 0";

            firstIncorrectQuestionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            secondIncorrectQuestionId = Guid.Parse("2222CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            thirdIncorrectQuestionId = Guid.Parse("3333CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            firstCorrectQuestionId = Guid.Parse("1111EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            secondCorrectQuestionId = Guid.Parse("2222EEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = firstIncorrectQuestionId,
                    ValidationExpression = InvalidExpression,
                    ValidationMessage = "some message",
                    StataExportCaption = "var1"
                },
                new NumericQuestion
                {
                    PublicKey = secondIncorrectQuestionId,
                    ValidationExpression = InvalidExpression,
                    ValidationMessage = "some message",
                    StataExportCaption = "var2"
                },
                new NumericQuestion
                {
                    PublicKey = thirdIncorrectQuestionId,
                    ValidationExpression = InvalidExpression,
                    ValidationMessage = "some message",
                    StataExportCaption = "var3"
                },
                new NumericQuestion
                {
                    PublicKey = firstCorrectQuestionId,
                    ValidationExpression = ValidExpression,
                    ValidationMessage = "some message",
                    StataExportCaption = "var4"
                },
                new NumericQuestion
                {
                    PublicKey = secondCorrectQuestionId,
                    ValidationExpression = ValidExpression,
                    ValidationMessage = "some message",
                    StataExportCaption = "var5"
                }
                );

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_3_messages = () =>
            verificationMessages.Count().ShouldEqual(3);

        It should_return_messages_each_with_code__WB0002__ = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.Code == "WB0002");

        It should_return_messages_each_having_single_reference = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_messages_each_referencing_question = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_message_referencing_first_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_message_referencing_secong_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == secondIncorrectQuestionId);

        It should_return_message_referencing_third_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == thirdIncorrectQuestionId);

        It should_not_return_error_referencing_first_correct_question = () =>
            verificationMessages.ShouldNotContain(error
                => error.References.Single().Id == firstCorrectQuestionId);

        It should_not_return_error_referencing_second_correct_question = () =>
            verificationMessages.ShouldNotContain(error
                => error.References.Single().Id == secondCorrectQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid thirdIncorrectQuestionId;
        private static Guid firstCorrectQuestionId;
        private static Guid secondCorrectQuestionId;
    }
}