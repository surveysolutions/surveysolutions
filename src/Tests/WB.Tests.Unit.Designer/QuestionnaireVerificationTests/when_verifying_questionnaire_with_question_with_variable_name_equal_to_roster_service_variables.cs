using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_variable_name_equal_to_roster_service_variables : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowcode",
                QuestionText = "hello rowcode"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowname",
                QuestionText = "hello rowname"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowindex",
                QuestionText = "hello rowindex"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "roster",
                QuestionText = "hello roster"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "Id",
                QuestionText = "hello Id"
            });
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_5_error () =>
            verificationMessages.Count().Should().Be(5);

        [NUnit.Framework.Test] public void should_return_all_errors_with_code__WB0058 () =>
            verificationMessages.Should().OnlyContain(e=>e.Code=="WB0058");

        [NUnit.Framework.Test] public void should_return_all_errors_with_1_references () =>
            verificationMessages.Should().OnlyContain(e=>e.References.Count==1);

        [NUnit.Framework.Test] public void should_return_all_errors_reference_with_type_Question () =>
            verificationMessages.Should().OnlyContain(e=>e.References.First().Type==QuestionnaireVerificationReferenceType.Question);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}
