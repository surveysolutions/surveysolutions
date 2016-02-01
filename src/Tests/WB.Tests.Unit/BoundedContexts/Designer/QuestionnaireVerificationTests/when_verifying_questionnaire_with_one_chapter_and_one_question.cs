using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_one_chapter_and_one_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion() {StataExportCaption = "var"}
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.Verify(questionnaire);

        It should_return_no_errors = () =>
            verificationMessages.ShouldBeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static IQuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}