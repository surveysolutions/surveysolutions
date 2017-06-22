using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_question_that_has_empty_variable_name : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion("Question with empty var")
                    {
                        PublicKey = questionWithEmptyVarId
                    });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0057 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0057");

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single()
                .References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_questionWithEmptyVar () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(questionWithEmptyVarId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid questionWithEmptyVarId = Guid.Parse("11111111111111111111111111111111");
    }
}
