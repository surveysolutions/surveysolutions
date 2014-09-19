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
    internal class when_verifying_questionnaire_with_2_multimedia_questions_with_supervisor_and_prefilled_scope_respectively : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new MultimediaQuestion()
            {
                PublicKey = supervisorQuestionId,
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "var1"
            },
            new MultimediaQuestion()
            {
                PublicKey = hqQuestionId,
                QuestionScope = QuestionScope.Headquarter,
                StataExportCaption = "var2"
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_first_error_with_code__WB0078 = () =>
            resultErrors.First().Code.ShouldEqual("WB0078");

        It should_return_first_error_with_1_references = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_supervisorQuestionId = () =>
            resultErrors.First().References.First().Id.ShouldEqual(supervisorQuestionId);

        It should_return_second_error_with_code__WB0078 = () =>
            resultErrors.Last().Code.ShouldEqual("WB0078");

        It should_return_second_error_with_1_references = () =>
            resultErrors.Last().References.Count().ShouldEqual(1);

        It should_return_second_error_reference_with_type_Question = () =>
            resultErrors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_hqQuestionId = () =>
            resultErrors.Last().References.First().Id.ShouldEqual(hqQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid supervisorQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid hqQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}
