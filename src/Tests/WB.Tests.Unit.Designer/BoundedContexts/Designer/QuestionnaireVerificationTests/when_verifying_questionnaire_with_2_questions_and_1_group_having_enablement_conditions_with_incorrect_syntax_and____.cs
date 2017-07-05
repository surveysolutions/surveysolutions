using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_questions_and_1_group_having_enablement_conditions_with_incorrect_syntax_and_1_question_and_1_group_having_correct_syntax :
            QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            const string InvalidExpression = "[hehe] &=+< 5";
            const string ValidExpression = "var1 > 0";

            firstIncorrectQuestionId = Guid.Parse("11111111111111111111111111111111");
            secondIncorrectQuestionId = Guid.Parse("22222222222222222222222222222222");
            incorrectGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            correctQuestionId = Guid.Parse("33333333333333333333333333333333");
            correctGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion("text 1")
                {
                    PublicKey = firstIncorrectQuestionId,
                    ConditionExpression = InvalidExpression,
                    StataExportCaption = "var1"
                },
                new TextQuestion("text 1")
                {
                    PublicKey = secondIncorrectQuestionId,
                    ConditionExpression = InvalidExpression,
                    StataExportCaption = "var2"
                },
                new Group { PublicKey = incorrectGroupId, ConditionExpression = InvalidExpression },
                new NumericQuestion("text 1")
                {
                    PublicKey = correctQuestionId,
                    ConditionExpression = ValidExpression,
                    StataExportCaption = "var3"
                },
                new Group { PublicKey = correctGroupId, ConditionExpression = ValidExpression }
                );

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToArray();

        It should_return_3_messages = () =>
            verificationMessages.Count().ShouldEqual(3);

        It should_return_messages_each_with_code__WB0003__ = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.Code == "WB0003");

        It should_return_messages_each_having_single_reference = () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_message_referencing_first_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_message_referencing_second_incorrect_question = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == secondIncorrectQuestionId);

        It should_return_message_referencing_incorrect_group = () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Group
                    && error.References.Single().Id == incorrectGroupId);

        It should_not_return_error_referencing_correct_question = () =>
            verificationMessages.ShouldNotContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == correctQuestionId);

        It should_not_return_error_referencing_correct_group = () =>
            verificationMessages.ShouldNotContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Group
                    && error.References.Single().Id == correctGroupId);

        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid incorrectGroupId;
        private static Guid correctQuestionId;
        private static Guid correctGroupId;
    }
}