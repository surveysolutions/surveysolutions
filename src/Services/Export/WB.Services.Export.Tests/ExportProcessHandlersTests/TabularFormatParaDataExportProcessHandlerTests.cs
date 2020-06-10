﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.ExportProcessHandlersTests
{
    [TestOf(typeof(TabularFormatParaDataExportProcessHandler))]
    public class TabularFormatParaDataExportProcessHandlerTestsContext
    {
        protected TabularFormatParaDataExportProcessHandler CreateTabularFormatParaDataExportProcessHandler(
            IOptions<ExportServiceSettings> interviewDataExportSettings = null,
            ITenantApi<IHeadquartersApi> tenantApi = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IInterviewsToExportSource interviewsToExportSource = null,
            ICsvWriter csvWriter = null,
            ILogger<TabularFormatParaDataExportProcessHandler> logger = null)
        {
            return new TabularFormatParaDataExportProcessHandler(
                interviewDataExportSettings ?? Mock.Of<IOptions<ExportServiceSettings>>(),
                tenantApi ?? Mock.Of<ITenantApi<IHeadquartersApi>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                interviewsToExportSource ?? Mock.Of<IInterviewsToExportSource> (),
                csvWriter ?? Mock.Of<ICsvWriter> (),
                logger ?? Mock.Of<ILogger<TabularFormatParaDataExportProcessHandler>>());
        }
    }

    public class TabularFormatParaDataExportProcessHandlerTests : TabularFormatParaDataExportProcessHandlerTestsContext
    {
        [Test]
        public async Task when_creating_paradata_with_do_file()
        {
            List<string> writed = new List<string>();
            var mockOfFileSystemAccessor = new Mock<IFileSystemAccessor>();
            
            mockOfFileSystemAccessor.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string a, string stringToWrite) =>
                {
                    writed.Add(stringToWrite);
                });
            
            var interviewDataExportSettings = new Mock<IOptions<ExportServiceSettings>>();
            interviewDataExportSettings.Setup(x=> x.Value).Returns(new ExportServiceSettings() { MaxRecordsCountPerOneExportQuery = 1});

            var interviewsToExportSource = new Mock<IInterviewsToExportSource>();
            interviewsToExportSource.Setup(x => x.GetInterviewsToExport(It.IsAny<QuestionnaireId>(),
                    It.IsAny<InterviewStatus?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()
                    ))
                .Returns(new List<InterviewToExport>());

            var csvWriter = new Mock<ICsvWriter>();
            csvWriter.Setup(x => x.OpenCsvWriter(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(new Mock<ICsvWriterService>().Object);
            
            var handler = CreateTabularFormatParaDataExportProcessHandler(
                interviewDataExportSettings: interviewDataExportSettings.Object,
                interviewsToExportSource: interviewsToExportSource.Object,
                fileSystemAccessor:mockOfFileSystemAccessor.Object,
                csvWriter: csvWriter.Object);

            var state = new ExportState(new DataExportProcessArgs()
            {
                ExportSettings = new ExportSettings(
                    exportFormat:DataExportFormat.Tabular,
                    new QuestionnaireId(Guid.Empty.ToString()),
                    new TenantInfo("http://test",""))
            });

            await handler.ExportDataAsync(state, CancellationToken.None);

            Assert.That(writed.First().Contains("label define role 0 `\"<UNKNOWN ROLE>\"' 1 `\"Interviewer\"' 2 `\"Supervisor\"' 3 `\"Headquarter\"' 4 `\"Administrator\"' 5 `\"API User\"'"));
        }
    }
}
