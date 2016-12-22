using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Security;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.SpssFormatExportHandlerTests
{
    [Subject(typeof(SpssFormatExportHandler))]
    internal class SpssFormatExportHandlerTestContext
    {
        protected static SpssFormatExportHandler CreateSpssFormatExportHandler(
            IFileSystemAccessor fileSystemAccessor = null,
            IZipArchiveProtectionService archiveUtils = null,
            ITabularFormatExportService tabularFormatExportService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService = null)
        {
            return new SpssFormatExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(_=>_.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()) ==new[] {"test.tab"}),
                archiveUtils ?? Mock.Of<IZipArchiveProtectionService>(),
                new InterviewDataExportSettings(),
                tabularFormatExportService ?? Mock.Of<ITabularFormatExportService>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                tabularDataToExternalStatPackageExportService ??
                Mock.Of<ITabularDataToExternalStatPackageExportService>(),
                Mock.Of<IDataExportProcessesService>(),
                Mock.Of<ILogger>(),
                Mock.Of<IExportSettings>());
        }
    }
}