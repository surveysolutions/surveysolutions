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
            questionId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion
            {
                PublicKey = questionId,
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

        It should_return_reference_to_question = () => 
            verificationErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_reference_with_id_of_question = () =>
            verificationErrors.First().References.First().Id.ShouldEqual(questionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationError> verificationErrors;
        static Guid questionId;
    }
}

