using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_10_text_questions_and_5_numeric_questions_with_no_validation_conditions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),
                Create.TextQuestion(),

                Create.NumericIntegerQuestion(),
                Create.NumericRealQuestion(),
                Create.NumericIntegerQuestion(),
                Create.NumericRealQuestion(),
                Create.NumericIntegerQuestion(),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            messages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_warning_WB0208 = () =>
            messages.ShouldContainWarning("WB0208");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}