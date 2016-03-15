using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_101_questions_and_2_sections_and_5_subsections : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Section(),
                Create.Section(children: new IComposite[]
                {
                    Create.Group(),
                    Create.Group(),
                    Create.Group(),
                    Create.Group(),
                    Create.Group(
                        children: Enumerable.Range(1, 101).Select(_ => Create.TextQuestion()).ToArray<IComposite>()),
                }),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            messages = verifier.Verify(questionnaire);

        It should_return_warning_WB0206 = () =>
            messages.ShouldContainWarning("WB0206");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}