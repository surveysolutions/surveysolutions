using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    [Subject(typeof(FileBasedDataExportService))]
    internal class FileBasedDataExportServiceTestContext
    {
        protected static FileBasedDataExportService CreateFileBasedDataExportService(
            IFileSystemAccessor fileSystemAccessor = null, IDataFileExportService dataFileExportService = null,
            IEnvironmentContentService environmentContentService = null, IDataFileService preloadedDataService = null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();
            return new FileBasedDataExportService(Mock.Of<IReadSideRepositoryCleanerRegistry>(), "",
                dataFileExportService ?? Mock.Of<IDataFileExportService>(),
                environmentContentService ?? Mock.Of<IEnvironmentContentService>(), currentFileSystemAccessor,
                preloadedDataService ?? new DataFileService(currentFileSystemAccessor),
                Mock.Of<ILogger>());
        }

        protected static void AddLevelToExportStructure(QuestionnaireExportStructure questionnaireExportStructure, Guid levelId,
            string levelName)
        {
            questionnaireExportStructure.HeaderToLevelMap.Add(levelId,
               new HeaderStructureForLevel() { LevelId = levelId, LevelName = levelName });
        }
    }
}
