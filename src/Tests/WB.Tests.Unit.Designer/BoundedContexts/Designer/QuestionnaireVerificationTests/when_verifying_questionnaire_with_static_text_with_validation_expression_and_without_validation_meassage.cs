using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_static_text_with_validation_expression_and_without_validation_meassage : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(),
                Create.StaticText(
                    staticTextId : staticTextId,
                    validationConditions: new List<ValidationCondition>() { new ValidationCondition(validationExpression, string.Empty)})
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_messages_each_with_code__WB00107__ = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0107");

        It should_return_messages_each_having_single_reference = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_messages_each_referencing_static_text = () =>
            verificationMessages.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_message_referencing_first_incorrect_question = () =>
            verificationMessages.Single().References.Single().Id.ShouldEqual(staticTextId);

        private static List<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid staticTextId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private const string validationExpression = "some expression";
    }
}