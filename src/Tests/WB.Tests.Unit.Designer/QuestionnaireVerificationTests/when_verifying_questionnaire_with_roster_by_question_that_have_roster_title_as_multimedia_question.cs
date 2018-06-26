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
    internal class when_verifying_questionnaire_with_roster_by_question_that_have_roster_title_as_multimedia_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("rosterSizeQuestion1")
                {
                    PublicKey = rosterSizeQuestionId,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new Group
                {
                    Title = "Roster 1. Triggered by rosterSizeQuestion2",
                    PublicKey = rosterId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleMultimediaQuestionId,
                    Children = new List<IComposite>()
                    {
                        new MultimediaQuestion("rosterTitleQuestion")
                        {
                            StataExportCaption = "var3",
                            PublicKey = rosterTitleMultimediaQuestionId
                        }
                    }.ToReadOnlyCollection()
                });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0035__ () =>
            verificationMessages.Single().Code.Should().Be("WB0083");

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Single().References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Roster () =>
            verificationMessages.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_rosterId () =>
           verificationMessages.Single().References.First().Id.Should().Be(rosterId);

        [NUnit.Framework.Test]
        public void should_return_second_message_reference_with_type_group() =>
            verificationMessages.Single().References.Second().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
        
        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_rosterTitleMultimediaQuestionId () =>
           verificationMessages.Single().References.Second().Id.Should().Be(rosterTitleMultimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId = Guid.Parse("10000000000000000000000000000000");
        private static Guid rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
        private static Guid rosterTitleMultimediaQuestionId = Guid.Parse("11333333333333333333333333333333");
    }
}
