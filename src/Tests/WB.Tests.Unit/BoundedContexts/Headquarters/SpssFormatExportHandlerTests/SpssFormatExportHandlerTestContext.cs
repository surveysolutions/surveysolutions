using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.SpssFormatExportHandlerTests
{
    [Subject(typeof(SpssFormatExportHandler))]
    internal class SpssFormatExportHandlerTestContext
    {
        protected static SpssFormatExportHandler CreateSpssFormatExportHandler(
            IFileSystemAccessor fileSystemAccessor = null,
            IArchiveUtils archiveUtils = null,
            ITabularFormatExportService tabularFormatExportService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService = null)
        {
            return new SpssFormatExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(_=>_.GetFilesInDirectory(Moq.It.IsAny<string>())==new[] {"test.tab"}),
                archiveUtils ?? Mock.Of<IArchiveUtils>(),
                new InterviewDataExportSettings(),
                tabularFormatExportService ?? Mock.Of<ITabularFormatExportService>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                tabularDataToExternalStatPackageExportService ??
                Mock.Of<ITabularDataToExternalStatPackageExportService>(),
                Mock.Of<IDataExportProcessesService>());
        }
    }
}