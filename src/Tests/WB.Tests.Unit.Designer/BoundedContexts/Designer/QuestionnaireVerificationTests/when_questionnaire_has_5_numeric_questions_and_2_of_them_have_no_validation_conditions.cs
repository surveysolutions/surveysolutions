using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_5_numeric_questions_and_2_of_them_have_no_validation_conditions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(validationConditions: new [] { Create.ValidationCondition() }),
                Create.NumericRealQuestion(validationConditions: new [] { Create.ValidationCondition() }),
                Create.NumericIntegerQuestion(validationConditions: new [] { Create.ValidationCondition() }),
                Create.NumericRealQuestion(),
                Create.NumericIntegerQuestion(),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            messages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_not_return_message_WB0208 = () =>
            messages.ShouldNotContainMessage("WB0208");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}