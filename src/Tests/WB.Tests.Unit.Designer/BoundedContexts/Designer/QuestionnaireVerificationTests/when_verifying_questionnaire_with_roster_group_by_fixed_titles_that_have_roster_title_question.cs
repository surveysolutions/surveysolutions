using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_by_fixed_titles_that_have_roster_title_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterTitleQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(
                new Group()
            {
                PublicKey = rosterGroupId,
                IsRoster = true,
                VariableName = "a",
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                RosterTitleQuestionId = rosterTitleQuestionId,
                FixedRosterTitles = new[] { Create.FixedRosterTitle(1, "fixed title 1"), Create.FixedRosterTitle(2, "fixed title 2") },
                Children = new List<IComposite>()
                {
                    new NumericQuestion("question 1")
                    {
                        PublicKey = rosterTitleQuestionId,
                        StataExportCaption = "var",
                        IsInteger = true
                    }
                }.ToReadOnlyCollection()
            });
            verifier = CreateQuestionnaireVerifier();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0034__ () =>
            verificationMessages.Single().Code.ShouldEqual("WB0034");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Roster () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroupId () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterTitleQuestionId;
    }
}
