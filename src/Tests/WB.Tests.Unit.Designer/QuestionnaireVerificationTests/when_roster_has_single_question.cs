using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_roster_has_single_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(Create.Roster(
                rosterId: rosterId,
                children: new[] {Create.Question()}));
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() { errors = verifier.Verify(Create.QuestionnaireView(questionnaire)); }

        [NUnit.Framework.Test] public void should_contain_WB0203_warning () => errors.Should().Contain(item => item.Code == "WB0203" && item.MessageLevel == VerificationMessageLevel.Warning);

        [NUnit.Framework.Test] public void should_reference_to_a_roster () => errors.FirstOrDefault(item => item.Code == "WB0203")
            .References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_reference_to_a_roster_with_single_question () => errors.FirstOrDefault(item => item.Code == "WB0203")
            .References.First().Id.Should().Be(rosterId);

        [NUnit.Framework.Test] public void should_add_single_reference () => errors.FirstOrDefault(item => item.Code == "WB0203").References.Count.Should().Be(1);

        static QuestionnaireVerifier verifier;
        static QuestionnaireDocument questionnaire;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid rosterId;
    }
}
