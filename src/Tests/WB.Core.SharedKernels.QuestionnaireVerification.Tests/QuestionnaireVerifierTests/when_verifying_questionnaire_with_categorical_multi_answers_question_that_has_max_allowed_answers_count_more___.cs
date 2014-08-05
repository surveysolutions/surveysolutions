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
    class when_verifying_questionnaire_with_categorical_multi_answers_question_that_has_max_allowed_answers_count_more_than_options_count : QuestionnaireVerifierTestsContext
    {

        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            
            questionnaire.Children.Add(new MultyOptionsQuestion()
            {
                PublicKey = multyOptionsQuestionId,
                StataExportCaption = "var",
                Answers = new List<Answer>() { new Answer() { AnswerValue = "2", AnswerText = "2" }, new Answer() { AnswerValue = "1", AnswerText = "1" } },
                MaxAllowedAnswers = 3
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_errors = () => 
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0021__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0021");

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;

        private static Guid multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
    }
}
