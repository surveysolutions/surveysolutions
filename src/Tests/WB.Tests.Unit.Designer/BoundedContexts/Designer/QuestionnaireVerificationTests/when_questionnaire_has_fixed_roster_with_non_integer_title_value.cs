using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_fixed_roster_with_non_integer_title_value : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.FixedRoster(fixedRosterTitles: new[] { new FixedRosterTitle(1.5m, "1.5"),  new FixedRosterTitle(11, "1")}),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            messages = verifier.CheckForErrors(questionnaire);

        It should_have_1_error = () =>
            messages.Count().ShouldEqual(1);

        It should_not_return_message_WB0115 = () =>
            messages.Single().Code.ShouldEqual("WB0115");

        It should_return_message_with_one_references = () =>
            messages.Single().References.Count().ShouldEqual(1);
       
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}