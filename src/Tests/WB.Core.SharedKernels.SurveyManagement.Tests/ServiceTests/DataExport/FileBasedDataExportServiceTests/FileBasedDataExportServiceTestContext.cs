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
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    [Subject(typeof(FileBasedDataExportRepositoryWriter))]
    internal class FileBasedDataExportServiceTestContext
    {
        protected static FileBasedDataExportRepositoryWriter CreateFileBasedDataExportService(
            IFileSystemAccessor fileSystemAccessor = null, IDataExportWriter dataExportWriter = null,
            IEnvironmentContentService environmentContentService = null, IPlainInterviewFileStorage plainFileRepository = null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();
            return new FileBasedDataExportRepositoryWriter(Mock.Of<IReadSideRepositoryCleanerRegistry>(), "",
                dataExportWriter ?? Mock.Of<IDataExportWriter>(),
                environmentContentService ?? Mock.Of<IEnvironmentContentService>(), currentFileSystemAccessor,
                Mock.Of<ILogger>(), plainFileRepository ?? Mock.Of<IPlainInterviewFileStorage>(_ => _.GetBinaryFilesForInterview(Moq.It.IsAny<Guid>()) == new List<InterviewBinaryDataDescriptor>()));
        }

        protected static void AddLevelToExportStructure(QuestionnaireExportStructure questionnaireExportStructure, Guid levelId,
            string levelName)
        {
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid> { levelId },
               new HeaderStructureForLevel() { LevelScopeVector = new ValueVector<Guid> { levelId }, LevelName = levelName });
        }
    }
}
