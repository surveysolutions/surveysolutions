using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_size_question_inside_dependent_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        Create.NumericIntegerQuestion(
                            rosterSizeQuestionId,
                            variable: "var"
                        )
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0054__ () =>
            verificationMessages.ShouldContainError("WB0054");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.GetError("WB0054").MessageLevel.Should().Be(VerificationMessageLevel.General);
        
        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.GetError("WB0054").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Roster () =>
            verificationMessages.GetError("WB0054").References.ElementAt(0).Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_rosterId () =>
            verificationMessages.GetError("WB0054").References.ElementAt(0).Id.Should().Be(rosterGroupId);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_question () =>
            verificationMessages.GetError("WB0054").References.ElementAt(1).Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_rosterSizeQuestionId () =>
            verificationMessages.GetError("WB0054").References.ElementAt(1).Id.Should().Be(rosterSizeQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
