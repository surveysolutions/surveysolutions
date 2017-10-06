using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_empty_static_text : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    Create.StaticText(staticTextId: staticTextId, text: string.Empty),
                    Create.TextQuestion(variable: "var")
                );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0071 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0071");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_questionId () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(staticTextId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid staticTextId = Guid.Parse("10000000000000000000000000000000");
    }
}
