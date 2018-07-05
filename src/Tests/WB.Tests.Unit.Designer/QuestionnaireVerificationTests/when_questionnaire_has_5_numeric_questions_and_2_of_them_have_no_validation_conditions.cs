using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_5_numeric_questions_and_2_of_them_have_no_validation_conditions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(validationConditions: new [] { Create.ValidationCondition() }),
                Create.NumericRealQuestion(validationConditions: new [] { Create.ValidationCondition() }),
                Create.NumericIntegerQuestion(validationConditions: new [] { Create.ValidationCondition() }),
                Create.NumericRealQuestion(),
                Create.NumericIntegerQuestion(),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            messages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_not_return_message_WB0208 () =>
            messages.ShouldNotContainMessage("WB0208");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}