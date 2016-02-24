using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExportStatusReaderTests
{
    [Subject(typeof(DataExportStatusReader))]
    internal class DataExportStatusReaderTestContext
    {
        protected static DataExportStatusReader CreateDataExportStatusReader(
            IDataExportProcessesService dataExportProcessesService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IParaDataAccessor paraDataAccessor = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader =null)
        {
            var questionnaireExportStructure = Create.QuestionnaireExportStructure();
            return new DataExportStatusReader(dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                paraDataAccessor ?? Mock.Of<IParaDataAccessor>(), fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireExportStructure>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireExportStructure));
        }
    }
}