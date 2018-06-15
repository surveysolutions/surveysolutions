using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_questions_and_1_group_having_enablement_conditions_with_incorrect_syntax_and_1_question_and_1_group_having_correct_syntax :
            QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
                new Group("Title") { PublicKey = incorrectGroupId, ConditionExpression = InvalidExpression },
                new NumericQuestion("text 1")
                {
                    PublicKey = correctQuestionId,
                    ConditionExpression = ValidExpression,
                    StataExportCaption = "var3"
                },
                new Group("Title1") { PublicKey = correctGroupId, ConditionExpression = ValidExpression }
                );

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToArray();

        [NUnit.Framework.Test] public void should_return_messages_each_with_code__WB0003__ () =>
            verificationMessages.ShouldContainError("WB0003");

        [NUnit.Framework.Test] public void should_return_messages_each_having_single_reference () =>
            verificationMessages.Should().OnlyContain(error
                => error.References.Count() == 1);

        [NUnit.Framework.Test] public void should_return_message_referencing_first_incorrect_question () =>
            verificationMessages.Should().Contain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == firstIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_return_message_referencing_second_incorrect_question () =>
            verificationMessages.Should().Contain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == secondIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_return_message_referencing_incorrect_group () =>
            verificationMessages.Should().Contain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Group
                    && error.References.Single().Id == incorrectGroupId);

        [NUnit.Framework.Test] public void should_not_return_error_referencing_correct_question () =>
            verificationMessages.Should().NotContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == correctQuestionId);

        [NUnit.Framework.Test] public void should_not_return_error_referencing_correct_group () =>
            verificationMessages.Should().NotContain(error
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
