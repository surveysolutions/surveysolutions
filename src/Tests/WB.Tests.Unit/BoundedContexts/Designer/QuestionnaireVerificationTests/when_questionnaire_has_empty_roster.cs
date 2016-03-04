using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_empty_roster : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.Roster(rosterId: rosterId)
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => messages = verifier.Verify(questionnaire);

        It should_return_warning_WB0204 = () =>
            messages.ShouldContainWarning("WB0204");

        It should_return_put_reference_to_empty_roster_to_message_WB0204 = () =>
            messages.GetWarning("WB0204").References.Single().Id.ShouldEqual(rosterId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
        private static Guid rosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}