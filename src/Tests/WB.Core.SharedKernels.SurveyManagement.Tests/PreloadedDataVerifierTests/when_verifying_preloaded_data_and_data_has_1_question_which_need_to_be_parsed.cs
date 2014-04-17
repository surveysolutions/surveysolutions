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
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_1_question_which_need_to_be_parsed : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionId = Guid.Parse("21111111111111111111111111111111");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new TextQuestion() { StataExportCaption = "q1", PublicKey = questionId });
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "q1", "ParentId" }, new string[][] { new string[] { "1", "text", "" } },
                "questionnaire.csv");

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_error = () =>
            result.Count().ShouldEqual(1);

        It should_error_has_code_PL0005 = () =>
            result.First().Code.ShouldEqual("PL0005");

        It should_error_has_type_of_reference_cell = () =>
            result.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid questionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}
