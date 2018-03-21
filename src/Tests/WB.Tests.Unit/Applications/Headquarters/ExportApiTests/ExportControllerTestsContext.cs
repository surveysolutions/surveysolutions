using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class ExportControllerTestsContext
    {
        public static ExportController CreateExportController(
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            IDataExportProcessesService dataExportProcessesService = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IDdiMetadataAccessor ddiMetadataAccessor = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IDataExportStatusReader dataExportStatusReader = null)
        {
            return new ExportController(
                questionnaireBrowseViewFactory: questionnaireBrowseViewFactory ?? Substitute.For<IQuestionnaireBrowseViewFactory>(),
                dataExportProcessesService: dataExportProcessesService ?? Substitute.For<IDataExportProcessesService>(),
                fileSystemAccessor: fileSystemAccessor ?? Substitute.For<IFileSystemAccessor>(),
                ddiMetadataAccessor: ddiMetadataAccessor ?? Substitute.For<IDdiMetadataAccessor>(),
                filebasedExportedDataAccessor: filebasedExportedDataAccessor ?? Substitute.For<IFilebasedExportedDataAccessor>(),
                dataExportStatusReader: dataExportStatusReader ?? Substitute.For<IDataExportStatusReader>());
        }
    }
}
