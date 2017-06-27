using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_validation_expression_and_without_validation_message : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericRealQuestion(
                    questionId,
                    validationExpression: validationExpression,
                    variable: "var1"
                ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_messages_each_with_code__WB00107__ () =>
            verificationMessages.ShouldContain(x =>x.Code == "WB0107");

        [NUnit.Framework.Test] public void should_return_messages_each_having_single_reference () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_messages_each_referencing_question () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_referencing_first_incorrect_question () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Single().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId;
        private const string validationExpression = "some expression";
    }
}