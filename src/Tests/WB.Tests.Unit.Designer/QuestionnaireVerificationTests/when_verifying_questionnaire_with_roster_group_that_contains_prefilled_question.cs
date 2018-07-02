using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_that_contains_prefilled_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            prefilledQuestionId = Guid.Parse("30000000000000000000000000000000");
            var rosterSizeQiestionId = Guid.Parse("20000000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocument(
                Create.NumericIntegerQuestion(
                    rosterSizeQiestionId,
                    variable: "var1"
                ),
                new Group()
                {
                    PublicKey = Guid.Parse("10000000000000000000000000000000"),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQiestionId,
                    Children = new List<IComposite>()
                    {
                        new TextQuestion("Title")
                        {
                            PublicKey = prefilledQuestionId, Featured = true, StataExportCaption = "var2"
                        }
                    }.ToReadOnlyCollection()
                });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));


        [NUnit.Framework.Test] public void should_return_message_with_code__WB0029__ () =>
            verificationMessages.ShouldContainError("WB0030");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.GetError("WB0030").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_group () =>
            verificationMessages.GetError("WB0030").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroupId () =>
            verificationMessages.GetError("WB0030").References.First().Id.Should().Be(prefilledQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid prefilledQuestionId;
    }
}
