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
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_enablement_condition : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion
                {
                    PublicKey = questionId,
                    ConditionExpression = "[hehe] &=+< 5",
                    StataExportCaption = "var1"
                });

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0003__ = () =>
            verificationMessages.First().Code.ShouldEqual("WB0003");

        It should_return_message_with_single_reference = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_message_referencing_with_type_of_question = () =>
            verificationMessages.First().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_referencing_with_specified_question_id = () =>
            verificationMessages.First().References.Single().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}