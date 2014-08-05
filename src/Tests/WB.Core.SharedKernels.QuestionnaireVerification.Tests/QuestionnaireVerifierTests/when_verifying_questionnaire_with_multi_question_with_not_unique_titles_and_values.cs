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
    internal class when_verifying_questionnaire_with_multi_question_with_not_unique_titles_and_values : QuestionnaireVerifierTestsContext
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
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerValue = "1", AnswerText = "1    " }, 
                        new Answer { AnswerValue = "   1", AnswerText = "1" }
                    },
                    QuestionType = QuestionType.MultyOption
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_error = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_error_with_code__WB0072__and__WB0073 = () =>
            resultErrors.Select(x => x.Code).ShouldContainOnly("WB0072", "WB0073");

        It should_return_first_error_with_1_references = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_second_error_with_1_references = () =>
            resultErrors.Last().References.Count().ShouldEqual(1);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_type_Question = () =>
            resultErrors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_equals_multiQuestionId = () =>
            resultErrors.First().References.First().Id.ShouldEqual(multiQuestionId);

        It should_return_second_error_reference_with_id_equals_multiQuestionId = () =>
            resultErrors.Last().References.First().Id.ShouldEqual(multiQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multiQuestionId;
    }
}