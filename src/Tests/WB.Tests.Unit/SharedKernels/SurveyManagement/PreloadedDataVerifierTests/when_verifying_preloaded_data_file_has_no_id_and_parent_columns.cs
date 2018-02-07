using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_no_id_and_parent_columns : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            questionnaireId = Guid.Parse("11111111111111111111111111111111");

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
        };

        Because of =
            () => VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(CreatePreloadedDataByFile(new string[0], null, QuestionnaireCsvFileName)), preloadedDataServiceMock.Object).ToList();

        It should_result_has_1_error = () =>
            VerificationErrors.Count().ShouldEqual(1);

        It should_return_first_PL0007_error = () =>
            VerificationErrors.First().Code.ShouldEqual("PL0007");

        It should_return_second_PL0007_error = () =>
            VerificationErrors.Last().Code.ShouldEqual("PL0007");

        It should_return_reference_of_first_error_with_Column_type = () =>
            VerificationErrors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_return_reference_of_second_error_with_Column_type = () =>
            VerificationErrors.Last().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        
        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
    }
}
