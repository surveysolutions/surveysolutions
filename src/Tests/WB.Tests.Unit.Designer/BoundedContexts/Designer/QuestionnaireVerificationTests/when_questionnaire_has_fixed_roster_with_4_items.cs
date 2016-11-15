using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_fixed_roster_with_4_items : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.FixedRoster(fixedTitles: new[] { "1", "2", "3", "4" }),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            messages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_not_return_message_WB0207 = () =>
            messages.ShouldNotContainMessage("WB0207");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}