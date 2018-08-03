using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_that_has_two_macro_with_same_names : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(questionId, Create.TextQuestion(variable: "var"));
            questionnaire.Macros.Add(macro1Id, Create.Macro("hello", "var == \"\""));
            questionnaire.Macros.Add(macro2Id, Create.Macro("hello", "var == \"\""));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0020 () =>
            verificationMessages.Single().Code.Should().Be("WB0020");

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.Single().References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Macro () =>
            verificationMessages.Single().References.Should().OnlyContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Macro);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_macro1 () =>
            verificationMessages.Single().References.ElementAt(0).Id.Should().Be(macro1Id);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_macro2 () =>
            verificationMessages.Single().References.ElementAt(1).Id.Should().Be(macro2Id);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid macro1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macro2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");

    }
}