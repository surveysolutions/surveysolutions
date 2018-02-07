using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_empty_responsible_name : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "_responsible" }, new string[][] { new string[] { "1", "" } },
                QuestionnaireCsvFileName);

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel(){ LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.AssignmentsCountColumnName)).Returns(-1);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.ResponsibleColumnName)).Returns(1);


            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
        };

        Because of =
            () => VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataServiceMock.Object).ToList();

        It should_result_has_1_error = () =>
            VerificationErrors.Count().ShouldEqual(1);

        It should_return_single_PL0006_error = () =>
            VerificationErrors.First().Code.ShouldEqual("PL0025");

        It should_return_reference_with_Cell_type = () =>
            VerificationErrors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_PositionX_be_equal_to_0 = () =>
            VerificationErrors.First().References.First().PositionX.ShouldEqual(1);

        It should_error_PositionY_be_equal_to_1 = () =>
            VerificationErrors.First().References.First().PositionY.ShouldEqual(0);

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
    }
}
