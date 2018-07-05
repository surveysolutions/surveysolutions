using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_roster_with_variable_name_equal_to_question_variable_name : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId,
                    fixedTitles: new[] {"1", "2"},
                    variable: nonUniqueVariableName,
                    children: new IComposite[]
                    {new TextListQuestion() {PublicKey = Guid.NewGuid(), StataExportCaption = "var1"}}),
                new TextQuestion()
                {
                    PublicKey = questionId,
                    QuestionType = QuestionType.Text,
                    StataExportCaption = nonUniqueVariableName,
                    QuestionText = "text question"
                },
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0026_error () =>
           verificationMessages.ShouldContainCritical("WB0026");

        [NUnit.Framework.Test] public void should_return_message_with_level_critical () =>
            verificationMessages.GetCritical("WB0026").MessageLevel.Should().Be(VerificationMessageLevel.Critical);

        [NUnit.Framework.Test] public void should_return_message_with_two_references () =>
            verificationMessages.GetCritical("WB0026").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_with_first_references_with_Roster_type () =>
            verificationMessages.GetCritical("WB0026").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_with_first_references_with_id_equals_rosterId () =>
            verificationMessages.GetCritical("WB0026").References.First().Id.Should().Be(rosterId);

        [NUnit.Framework.Test] public void should_return_message_with_second_references_with_Question_type () =>
            verificationMessages.GetCritical("WB0026").References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_second_references_with_id_equals_questionId () =>
            verificationMessages.GetCritical("WB0026").References.Last().Id.Should().Be(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");

        private static string nonUniqueVariableName = "variable";
    }
}
