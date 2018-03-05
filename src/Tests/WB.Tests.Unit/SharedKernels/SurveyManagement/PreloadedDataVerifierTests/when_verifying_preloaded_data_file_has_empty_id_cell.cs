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
    internal class when_verifying_preloaded_data_file_has_empty_id_cell : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId}, new string[][] { new string[] { "" } },
                QuestionnaireCsvFileName);

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.GetIdColumnIndex(preloadedDataByFile)).Returns(0);
            preloadedDataServiceMock.Setup(x => x.GetParentIdColumnIndexes(preloadedDataByFile)).Returns(new []{1});
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, Moq.It.IsAny<string>())).Returns(-1);
            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
        };

        Because of =
            () => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

        It should_result_has_1_error = () =>
            status.VerificationState.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0006_error = () =>
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0006");

        It should_return_reference_with_Cell_type = () =>
            status.VerificationState.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_has_content_empty_string = () =>
            status.VerificationState.Errors.First().References.First().Content.ShouldEqual("");

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;
        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
    }
}
