using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_CreateExportedDataStructureByTemplate_is_called_and_questionnaire_has_2_rosters_with_the_same_name_but_different_cases :
        FileBasedDataExportServiceTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>())).Returns<string>((fileName) => fileName);
            environmentContentService = new Mock<IEnvironmentContentService>();
            dataFileExportService = new Mock<IDataFileExportService>();

            var level1 = Guid.NewGuid();
            var level2 = Guid.NewGuid();

            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object, dataFileExportService.Object,
                environmentContentService.Object);

            questionnaireExportStructure = new QuestionnaireExportStructure();

            AddLevelToExportStructure(questionnaireExportStructure, level1, "level");
            AddLevelToExportStructure(questionnaireExportStructure, level2, "Level");
        };

        private Because of = () => fileBasedDataExportService.CreateExportedDataStructureByTemplate(questionnaireExportStructure);

        private It should_GetEnvironmentContentFileName_be_called_with_parameter_level = () =>
            environmentContentService.Verify(x => x.GetEnvironmentContentFileName("level"));

        private It should_GetEnvironmentContentFileName_be_called_with_parameter_level1 = () =>
            environmentContentService.Verify(x => x.GetEnvironmentContentFileName("level1"));

        private It should_GetInterviewExportedDataFileName_be_called_with_parameter_level = () =>
            dataFileExportService.Verify(x => x.GetInterviewExportedDataFileName("level"));

        private It should_GetInterviewExportedDataFileName_be_called_with_parameter_level1 = () =>
            dataFileExportService.Verify(x => x.GetInterviewExportedDataFileName("level1"));

        private static FileBasedDataExportService fileBasedDataExportService;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<IEnvironmentContentService> environmentContentService;
        private static Mock<IDataFileExportService> dataFileExportService;
    }
}
