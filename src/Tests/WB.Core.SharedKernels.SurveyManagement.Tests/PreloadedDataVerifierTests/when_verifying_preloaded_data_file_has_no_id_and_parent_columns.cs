using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_no_id_and_parent_columns : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, new[] { CreatePreloadedDataByFile(new string[0], null, "questionnaire.csv") });

        It should_result_has_2_error = () =>
           result.Count().ShouldEqual(2);

        It should_firts_error_has_code_PL0007 = () =>
            result.First().Code.ShouldEqual("PL0007");

        It should_last_error_has_code_PL0007 = () =>
            result.Last().Code.ShouldEqual("PL0007");

        It should_firts_error_has_type_of_reference_file = () =>
            result.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_last_error_has_type_of_reference_file = () =>
            result.Last().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_firt_error_has_content_with_id = () =>
            result.First().References.First().Content.ShouldEqual("Id");

        It should_second_error_has_content_with_id = () =>
            result.Last().References.First().Content.ShouldEqual("ParentId");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
    }
}
