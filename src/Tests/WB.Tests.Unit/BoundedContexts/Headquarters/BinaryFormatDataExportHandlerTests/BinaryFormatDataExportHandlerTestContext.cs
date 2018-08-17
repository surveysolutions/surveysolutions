using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests
{
    [NUnit.Framework.TestOf(typeof(BinaryFormatDataExportHandler))]
    internal class BinaryFormatDataExportHandlerTestContext
    {
        protected static BinaryFormatDataExportHandler CreateBinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor=null,
            IImageFileStorage imageFileRepository = null,
            IAudioFileStorage audioFileRepository = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries = null,
            IProtectedArchiveUtils archiveUtils = null,
            IInterviewFactory interviewFactory = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IDataExportProcessesService dataExportProcessesService = null,
            IDataExportFileAccessor dataExportFileAccessor = null,
            IAudioFileStorage audioFileStorage = null,
            InterviewDataExportSettings interviewDataExportSettings = null)
        {
            return new BinaryFormatDataExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                imageFileRepository ?? Mock.Of<IImageFileStorage>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                interviewDataExportSettings ?? new InterviewDataExportSettings(),
                interviewFactory ?? Mock.Of<IInterviewFactory>(),
                dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(), 
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                dataExportFileAccessor: dataExportFileAccessor ?? Mock.Of<IDataExportFileAccessor>(),
                audioFileStorage: audioFileRepository ?? Mock.Of<IAudioFileStorage>());
        }

        public static IDataExportFileAccessor CrerateDataExportFileAccessor(IFileSystemAccessor fileSystemAccessor = null,
            IProtectedArchiveUtils archiveUtils = null)
        {
            return new DataExportFileAccessor(Mock.Of<IExportSettings>(),
                archiveUtils ?? Mock.Of<IProtectedArchiveUtils>(),
                Mock.Of<ILogger>(),
                Mock.Of<IExternalFileStorage>());
        }
    }
}
