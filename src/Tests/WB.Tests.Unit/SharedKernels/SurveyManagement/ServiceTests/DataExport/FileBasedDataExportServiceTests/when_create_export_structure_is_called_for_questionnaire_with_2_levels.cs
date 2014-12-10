using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_create_export_structure_is_called_for_questionnaire_with_2_levels : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            environmentContentServiceMock=new Mock<IEnvironmentContentService>();

            questionnaireExportStructure = CreateQuestionnaireExportStructure();
            AddLevelToExportStructure(questionnaireExportStructure,Guid.NewGuid(),levelName1);
            AddLevelToExportStructure(questionnaireExportStructure, Guid.NewGuid(), levelName2);

            filebaseExportDataAccessorMock = new Mock<IFilebasedExportedDataAccessor>();

            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(filebasedExportedDataAccessor: filebaseExportDataAccessorMock.Object, environmentContentService: environmentContentServiceMock.Object);
        };

        Because of = () => fileBasedDataExportRepositoryWriter.CreateExportStructureByTemplate(questionnaireExportStructure);

        It should_create_data_folder = () =>
            filebaseExportDataAccessorMock.Verify(x => x.CreateExportDataFolder(questionnaireExportStructure.QuestionnaireId,questionnaireExportStructure.Version), Times.Once());

        It should_create_files_folder = () =>
            filebaseExportDataAccessorMock.Verify(x => x.CreateExportFileFolder(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version), Times.Once());

        It should_create_environment_files_for_first_level = () =>
            environmentContentServiceMock.Verify(x => x.CreateContentOfAdditionalFile(Moq.It.IsAny<HeaderStructureForLevel>(), "level test1.tab",
             Moq.It.IsAny<string>()), Times.Once());

        It should_create_environment_files_for_second_level = () =>
            environmentContentServiceMock.Verify(x => x.CreateContentOfAdditionalFile(Moq.It.IsAny<HeaderStructureForLevel>(), "level test2.tab",
            Moq.It.IsAny<string>()), Times.Once());

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;
        private static Mock<IFilebasedExportedDataAccessor> filebaseExportDataAccessorMock;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Mock<IEnvironmentContentService> environmentContentServiceMock;

        private static string levelName1 = "level test1";
        private static string levelName2 = "level test2";
    }
}
