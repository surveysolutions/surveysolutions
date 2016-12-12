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
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_validation_and_condition : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            const string invalidExpression = "[hehe] &=+< 5";
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion
                {
                    PublicKey = questionId,
                    ValidationExpression = invalidExpression,
                    ConditionExpression = invalidExpression, 
                    ValidationMessage = "validation message",
                    StataExportCaption = "var1"
                });

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        It should_return_2_messages = () =>
            verificationMessages.Count().ShouldEqual(2);

        It should_return_message_with_code__WB0003__ = () =>
            verificationMessages.Select(error => error.Code).ShouldContain("WB0003");

        It should_return_message_with_code__WB0002__ = () =>
            verificationMessages.Select(error => error.Code).ShouldContain("WB0002");

        It should_return_message_with_single_reference = () =>
            verificationMessages.ShouldEachConformTo(error=>error.References.Count() == 1);

        It should_return_message_referencing_with_type_of_question = () =>
            verificationMessages.ShouldEachConformTo(error=>error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_message_referencing_with_specified_question_id = () =>
            verificationMessages.ShouldEachConformTo(error=>error.References.Single().Id == questionId);

        private static List<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}