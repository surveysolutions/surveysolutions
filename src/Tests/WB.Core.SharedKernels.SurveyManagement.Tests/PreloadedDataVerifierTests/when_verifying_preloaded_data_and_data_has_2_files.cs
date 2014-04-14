using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_2_files : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, new[] { CreatePreloadedDataByFile(), CreatePreloadedDataByFile() });

        It should_error_has_code_PL0002 = () =>
            result.First().Code.ShouldEqual("PL0002");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
    }
}
