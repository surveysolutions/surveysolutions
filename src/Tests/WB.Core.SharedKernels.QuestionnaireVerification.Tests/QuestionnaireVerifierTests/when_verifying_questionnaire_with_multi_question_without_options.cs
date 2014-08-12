using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_multi_question_without_options : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            multiQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(
                new MultyOptionsQuestion
                {
                    PublicKey = multiQuestionId,
                    StataExportCaption = "var",
                    Answers = new List<Answer>(),
                    QuestionType = QuestionType.MultyOption
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_not_return_error_with_code__WB0072__and__WB0073 = () =>
            resultErrors.Select(x => x.Code).ShouldNotContain("WB0072", "WB0073");

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multiQuestionId;
    }
}