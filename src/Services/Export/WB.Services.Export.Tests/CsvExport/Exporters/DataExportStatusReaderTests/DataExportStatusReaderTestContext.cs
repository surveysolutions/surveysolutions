using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.DataExportTests.DataExportStatusReaderTests
{
    [NUnit.Framework.TestOf(typeof(DataExportStatusReader))]
    internal class DataExportStatusReaderTestContext
    {
        protected static DataExportStatusReader CreateDataExportStatusReader(
            IDataExportProcessesService dataExportProcessesService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IExternalFileStorage externalFileStorage = null,
            IQuestionnaireExportStructureStorage questionnaireReader =null)
        {
            var questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();

            return new DataExportStatusReader(dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), 
                Create.Fake.DataExportFileAccessor(),
                Mock.Of<IQuestionnaireExportStructureStorage>(
                    _ => _.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()) ==
                         questionnaireExportStructure), 
                externalFileStorage ?? Mock.Of<IExternalFileStorage>());
        }
    }
}
