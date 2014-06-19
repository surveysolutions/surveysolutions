using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_question_of_type_not_included_in_white_list_of_question_types:
        QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {

            questionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new SingleQuestion()
                {
                    PublicKey = questionId,
                    QuestionType = QuestionType.DropDownList,
                    StataExportCaption = questionId.ToString(),
                    Answers =
                        new List<Answer>
                        {
                            new Answer() { AnswerValue = "1", AnswerText = "1" },
                            new Answer() { AnswerValue = "2", AnswerText = "2" }
                        }
                }
                );

            verifier = CreateQuestionnaireVerifier();
        };

        private Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        private It should_return_1_errors = () =>
            resultErrors.Count().ShouldEqual(1);

        private It should_return_errors_each_with_code__WB0002__ = () =>
            resultErrors.ShouldEachConformTo(error
                => error.Code == "WB0066");

        private It should_return_errors_each_having_single_reference = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Count() == 1);

        private It should_return_errors_each_referencing_question = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        private It should_return_error_referencing_first_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == questionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId;
    }
}
