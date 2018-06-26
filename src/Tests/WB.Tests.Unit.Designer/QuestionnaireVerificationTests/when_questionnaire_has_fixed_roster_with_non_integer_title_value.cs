using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_fixed_roster_with_non_integer_title_value : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.FixedRoster(fixedRosterTitles: new[] { new FixedRosterTitle(1.5m, "1.5"),  new FixedRosterTitle(11, "1")}),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            messages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_have_1_error () =>
            messages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_not_return_message_WB0115 () =>
            messages.Single().Code.Should().Be("WB0115");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            messages.Single().References.Count().Should().Be(1);
       
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
    }
}