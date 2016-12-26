using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Security;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests
{
    [Subject(typeof(BinaryFormatDataExportHandler))]
    internal class BinaryFormatDataExportHandlerTestContext
    {
        protected static BinaryFormatDataExportHandler CreateBinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor=null,
            IPlainInterviewFileStorage plainFileRepository = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries = null,
            IZipArchiveProtectionService archiveUtils = null,
            IReadSideKeyValueStorage<InterviewData> interviewDatas = null,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null,
            IDataExportProcessesService dataExportProcessesService = null)
        {
            return new BinaryFormatDataExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                plainFileRepository ?? Mock.Of<IPlainInterviewFileStorage>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                new InterviewDataExportSettings(),
                Mock.Of<ITransactionManager>(),
                interviewSummaries ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                archiveUtils ?? Mock.Of<IZipArchiveProtectionService>(),
                interviewDatas ?? Mock.Of<IReadSideKeyValueStorage<InterviewData>>(),
                dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(),
                questionnaireExportStructureStorage: questionnaireExportStructureStorage ?? Mock.Of<IQuestionnaireExportStructureStorage>(),
                exportSettings: Mock.Of<IExportSettings>(),
                plainTransactionManagerProvider: Mock.Of<IPlainTransactionManagerProvider>(_ => _.GetPlainTransactionManager() == Mock.Of<IPlainTransactionManager>()));
        }
    }
}