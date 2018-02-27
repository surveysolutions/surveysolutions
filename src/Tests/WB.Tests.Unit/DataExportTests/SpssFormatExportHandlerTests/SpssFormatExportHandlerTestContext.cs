﻿using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Tests.Unit.DataExportTests.SpssFormatExportHandlerTests
{
    [Subject(typeof(SpssFormatExportHandler))]
    internal class SpssFormatExportHandlerTestContext
    {
        protected static SpssFormatExportHandler CreateSpssFormatExportHandler(
            IFileSystemAccessor fileSystemAccessor = null,
            IProtectedArchiveUtils archiveUtils = null,
            ITabularFormatExportService tabularFormatExportService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService = null,
            IDataExportFileAccessor dataExportFileAccessor = null)
        {
            return new SpssFormatExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(_=>_.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()) ==new[] {"test.tab"}),
                new InterviewDataExportSettings(),
                tabularFormatExportService ?? Mock.Of<ITabularFormatExportService>(),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>(),
                tabularDataToExternalStatPackageExportService ??
                Mock.Of<ITabularDataToExternalStatPackageExportService>(),
                Mock.Of<IDataExportProcessesService>(),
                Mock.Of<ILogger>(),
                dataExportFileAccessor ?? Mock.Of<IDataExportFileAccessor>());
        }

        public static IDataExportFileAccessor CrerateDataExportFileAccessor(IFileSystemAccessor fileSystemAccessor = null,
                IProtectedArchiveUtils archiveUtils = null)
        {
            return new DataExportFileAccessor(Mock.Of<IExportSettings>(),
                Mock.Of<IPlainTransactionManagerProvider>(_ => _.GetPlainTransactionManager() == Mock.Of<IPlainTransactionManager>()),
                archiveUtils ?? Mock.Of<IProtectedArchiveUtils>(),
                Mock.Of<ILogger>());
        }
    }
}