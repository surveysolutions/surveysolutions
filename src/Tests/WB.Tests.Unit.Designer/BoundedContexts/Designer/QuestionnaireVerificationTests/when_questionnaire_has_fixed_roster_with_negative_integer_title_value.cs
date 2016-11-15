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
    internal class when_questionnaire_has_fixed_roster_with_negative_integer_title_value : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.FixedRoster(fixedRosterTitles: new[] { new FixedRosterTitle(-1, "-1"),  new FixedRosterTitle(11, "11")}),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            messages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_have_0_error = () =>
            messages.Count().ShouldEqual(0);
      
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}