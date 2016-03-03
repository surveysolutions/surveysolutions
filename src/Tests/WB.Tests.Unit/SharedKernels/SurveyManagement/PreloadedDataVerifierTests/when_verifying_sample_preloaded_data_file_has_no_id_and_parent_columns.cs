using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_sample_preloaded_data_file_has_no_id_and_parent_columns : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            preloadedDataByFile = CreatePreloadedDataByFile(new string[0], new string[][] {new string[0]},
                QuestionnaireCsvFileName);
            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());

            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(Moq.It.IsAny<PreloadedDataByFile>(), "_responsible")).Returns(-1);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, null, preloadedDataServiceMock.Object);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.VerifySample(questionnaireId, 1, preloadedDataByFile);

        It should_result_has_no_errors = () =>
           result.Errors.ShouldBeEmpty();

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}
