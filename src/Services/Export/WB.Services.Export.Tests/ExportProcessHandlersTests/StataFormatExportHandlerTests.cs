using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.ExportProcessHandlersTests
{
    [TestOf(typeof(StataFormatExportHandler))]
    public class StataFormatExportHandlerTestsContext
    {
        protected StataFormatExportHandler CreateStataFormatExportHandler(
            IFileSystemAccessor fileSystemAccessor = null,
            IOptions<ExportServiceSettings> interviewDataExportSettings = null,
            ITabularFormatExportService tabularFormatExportService = null,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor = null,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService = null,
            IDataExportProcessesService dataExportProcessesService = null,
            ILogger<StataFormatExportHandler> logger = null,
            IDataExportFileAccessor dataExportFileAccessor = null)
        {
            return new StataFormatExportHandler(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                interviewDataExportSettings ?? Mock.Of<IOptions<ExportServiceSettings>>(),
                tabularFormatExportService ?? Mock.Of<ITabularFormatExportService>(),
                fileBasedExportedDataAccessor ?? Mock.Of<IFileBasedExportedDataAccessor>(),
                tabularDataToExternalStatPackageExportService ?? Mock.Of<ITabularDataToExternalStatPackageExportService>(),
                dataExportProcessesService ?? Mock.Of<IDataExportProcessesService>(),
                logger ?? Mock.Of<ILogger<StataFormatExportHandler>>(),
                dataExportFileAccessor ?? Mock.Of<IDataExportFileAccessor>());
        }
    }

    public class StataFormatExportHandlerTests  : StataFormatExportHandlerTestsContext
    {
        [Test]
        public void when_exported_file_lists_differs_from_original_list()
        {
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService = 
                Mock.Of<ITabularDataToExternalStatPackageExportService>(
                    t=> t.CreateAndGetSpssDataFilesForQuestionnaireAsync(It.IsAny<TenantInfo>(), It.IsAny<QuestionnaireId>(), 
                            It.IsAny<string[]>(), It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()) == Task.FromResult(new string[]{"test.sav"}));
            
            var mockOfFileSystemAccessor = new Mock<IFileSystemAccessor>();
            mockOfFileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>())).Returns(() => new[] { "test.tab", "test1.tab"});
            mockOfFileSystemAccessor.Setup(x => x.GetFileNameWithoutExtension(It.IsAny<string>())).Returns<string>(Path.GetFileNameWithoutExtension);
            
            var exporter = CreateStataFormatExportHandler(fileSystemAccessor: mockOfFileSystemAccessor.Object,
                tabularDataToExternalStatPackageExportService : tabularDataToExternalStatPackageExportService);
            
            Assert.ThrowsAsync<InvalidOperationException>(async () => await exporter.DoExportAsync(Mock.Of<DataExportProcessArgs>(), new ExportSettings(), 
                "test", new ExportProgress(), new CancellationToken()));
        }
    }
}
