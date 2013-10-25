using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.Verification;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_one_chapter_and_one_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion()
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_no_errors = () =>
            resultErrors.ShouldBeEmpty();

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static IQuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}