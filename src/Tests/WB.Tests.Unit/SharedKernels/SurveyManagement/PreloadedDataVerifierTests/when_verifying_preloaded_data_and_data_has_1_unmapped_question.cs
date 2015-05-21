using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_1_unmapped_question : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "q1" }, null, QuestionnaireCsvFileName);

            preloadedDataServiceMock=new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, null, preloadedDataServiceMock.Object);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_error = () =>
            result.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0003_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0003");

        It should_return_reference_with_Column_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;
        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
    }
}
