using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_macro_that_has_empty_name : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), Create.TextQuestion(variable: "var"));
            questionnaire.Macros.Add(macroId, Create.Macro(string.Empty, "var == \"\""));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0014 () =>
            verificationMessages.Single().Code.Should().Be("WB0014");

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Macro () =>
            verificationMessages.Single()
                .References.Should().OnlyContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Macro);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_macro () =>
            verificationMessages.Single().References.ElementAt(0).Id.Should().Be(macroId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid macroId = Guid.Parse("11111111111111111111111111111111");
    }
}