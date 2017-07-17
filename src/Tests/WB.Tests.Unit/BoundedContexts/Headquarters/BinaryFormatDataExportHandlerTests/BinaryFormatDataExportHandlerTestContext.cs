﻿using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
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
    [Subject(typeof(BinaryFormatDataExportHandler))]
    internal class BinaryFormatDataExportHandlerTestContext
    {
        protected static BinaryFormatDataExportHandler CreateBinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor=null,
            IFileSystemInterviewFileStorage fileSystemFileRepository = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries = null,
            IProtectedArchiveUtils archiveUtils = null,
            IReadSideKeyValueStorage<InterviewData> interviewDatas = null,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null,
            IDataExportProcessesService dataExportProcessesService = null,
            IDataExportFileAccessor dataExportFileAccessor = null)
        {
            return new BinaryFormatDataExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                fileSystemFileRepository ?? Mock.Of<IFileSystemInterviewFileStorage>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                new InterviewDataExportSettings(),
                Mock.Of<ITransactionManager>(),
                interviewSummaries ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                interviewDatas ?? Mock.Of<IReadSideKeyValueStorage<InterviewData>>(),
                dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(),
                questionnaireExportStructureStorage: questionnaireExportStructureStorage ?? Mock.Of<IQuestionnaireExportStructureStorage>(),
                dataExportFileAccessor: dataExportFileAccessor ?? Mock.Of<IDataExportFileAccessor>());
        }

        public static IDataExportFileAccessor CrerateDataExportFileAccessor(IFileSystemAccessor fileSystemAccessor = null,
            IProtectedArchiveUtils archiveUtils = null)
        {
            return new DataExportFileAccessor(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<IExportSettings>(),
                Mock.Of<IPlainTransactionManagerProvider>(_ => _.GetPlainTransactionManager() == Mock.Of<IPlainTransactionManager>()),
                archiveUtils ?? Mock.Of<IProtectedArchiveUtils>(),
                Mock.Of<ILogger>());
        }
    }
}