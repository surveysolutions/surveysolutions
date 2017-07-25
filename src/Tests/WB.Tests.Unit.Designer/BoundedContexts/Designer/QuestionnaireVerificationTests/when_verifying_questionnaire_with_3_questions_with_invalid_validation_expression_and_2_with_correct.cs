using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_3_questions_with_invalid_validation_expression_and_with_2_with_correct : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            const string InvalidExpression = "[hehe] &=+< 5";
            const string ValidExpression = "var1 > 0";

            firstIncorrectQuestionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            secondIncorrectQuestionId = Guid.Parse("2222CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            thirdIncorrectQuestionId = Guid.Parse("3333CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            firstCorrectQuestionId = Guid.Parse("1111EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            secondCorrectQuestionId = Guid.Parse("2222EEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericRealQuestion(
                    firstIncorrectQuestionId,
                    validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition(InvalidExpression, "some message")
                    },
                    variable: "var1"
                ),
                Create.NumericRealQuestion(
                    secondIncorrectQuestionId,
                    validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition(InvalidExpression, "some message")
                    },
                    variable: "var2"
                ),
                Create.NumericRealQuestion(
                    thirdIncorrectQuestionId,
                    validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition(InvalidExpression, "some message")
                    },
                    variable: "var3"
                ),
                Create.NumericRealQuestion(
                    firstCorrectQuestionId,
                    validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition(ValidExpression, "some message")
                    },
                    variable: "var4"
                ),
                Create.NumericRealQuestion(
                    secondCorrectQuestionId,
                    validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition(ValidExpression, "some message")
                    },
                    variable: "var5"
                )
                );

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_3_messages () =>
            verificationMessages.Count().ShouldEqual(3);

        [NUnit.Framework.Test] public void should_return_messages_each_with_code__WB0002__ () =>
            verificationMessages.ShouldEachConformTo(error
                => error.Code == "WB0002");

        [NUnit.Framework.Test] public void should_return_messages_each_having_single_reference () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Count() == 1);

        [NUnit.Framework.Test] public void should_return_messages_each_referencing_question () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_referencing_first_incorrect_question () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == firstIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_return_message_referencing_secong_incorrect_question () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == secondIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_return_message_referencing_third_incorrect_question () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == thirdIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_not_return_error_referencing_first_correct_question () =>
            verificationMessages.ShouldNotContain(error
                => error.References.Single().Id == firstCorrectQuestionId);

        [NUnit.Framework.Test] public void should_not_return_error_referencing_second_correct_question () =>
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