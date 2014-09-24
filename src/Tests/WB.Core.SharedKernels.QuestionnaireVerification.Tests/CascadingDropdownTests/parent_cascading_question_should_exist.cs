using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.CascadingDropdownTests
{
    internal class when_parent_cascading_question_missing_from_questionnaire : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                CascadeFromQuestionId = Guid.NewGuid(),
                StataExportCaption = "var",
                Answers = new List<Answer>
                {
                    new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                    new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationErrors = verifier.Verify(questionnaire).ToList();

        It should_return_WB0086_verification_error = () => verificationErrors.First().Code.ShouldEqual("WB0086");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationError> verificationErrors;
    }
}

