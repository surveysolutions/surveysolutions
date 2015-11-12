using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExportStatusReaderTests
{
    [Subject(typeof(DataExportStatusReader))]
    internal class DataExportStatusReaderTestContext
    {
        protected static DataExportStatusReader CreateDataExportStatusReader(
            IDataExportProcessesService dataExportProcessesService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IParaDataAccessor paraDataAccessor = null,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            return new DataExportStatusReader(dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                paraDataAccessor ?? Mock.Of<IParaDataAccessor>(), fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());
        }
    }
}