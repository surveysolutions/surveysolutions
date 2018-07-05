using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_that_referenced_by_question_with_substitutions_does_not_exist : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionWithNotExistingSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new SingleQuestion()
            {
                PublicKey = questionWithNotExistingSubstitutionsId,
                StataExportCaption = "var",
                QuestionText = "hello %unknown%!",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0017 () =>
            verificationMessages.Single().Code.Should().Be("WB0017");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_questionWithNotExistingSubstitutionsId () =>
            verificationMessages.Single().References.First().Id.Should().Be(questionWithNotExistingSubstitutionsId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithNotExistingSubstitutionsId;
    }
}
