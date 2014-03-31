using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    [Subject(typeof(FileBasedDataExportService))]
    internal class FileBasedDataExportServiceTestContext
    {
        protected static FileBasedDataExportService CreateFileBasedDataExportService(
            IFileSystemAccessor fileSystemAccessor = null, IDataFileExportService dataFileExportService = null,
            IEnvironmentContentService environmentContentService = null)
        {
            return new FileBasedDataExportService(Mock.Of<IReadSideRepositoryCleanerRegistry>(), "",
                dataFileExportService ?? Mock.Of<IDataFileExportService>(),
                environmentContentService ?? Mock.Of<IEnvironmentContentService>(), fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());
        }

        protected static void AddLevelToExportStructure(QuestionnaireExportStructure questionnaireExportStructure, Guid levelId,
            string levelName)
        {
            questionnaireExportStructure.HeaderToLevelMap.Add(levelId,
               new HeaderStructureForLevel() { LevelId = levelId, LevelName = levelName });
        }
    }
}
