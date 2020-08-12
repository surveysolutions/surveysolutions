using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Npgsql;
using NUnit.Framework;
using WB.Services.Export.Events;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Jobs;
using WB.Services.Export.Models;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.ExportJobTests
{
    public class ExportJobExecutionTests 
    {
        private TestLogger<ExportJob> logger;
        private Mock<IEventProcessor> eventProcessor;
        private Mock<IExportProcessHandler<DataExportProcessArgs>> handler;
        private Mock<IQuestionnaireSchemaGenerator> schemaGenerator;
        private ExportJob exportJob;

        [SetUp]
        public void Setup()
        {
            this.logger = new TestLogger<ExportJob>();
            this.eventProcessor = new Mock<IEventProcessor>();
            this.handler = new Mock<IExportProcessHandler<DataExportProcessArgs>>();
            this.schemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();

            this.exportJob = new ExportJob(new TenantContext(null, new TenantInfo("http://test","")), 
                eventProcessor.Object, logger, handler.Object, schemaGenerator.Object);
        }

        [TestCaseSource(nameof(nonLoggableExceptions))]
        public void should_not_log_Exceptions(Exception e)
        {
            this.handler.Setup(h =>
                    h.ExportDataAsync(It.IsAny<DataExportProcessArgs>(), It.IsAny<CancellationToken>()))
                .Throws(e);

            Assert.ThrowsAsync(e.GetType(), 
                () => this.exportJob.ExecuteAsync(Create.Entity.DataExportProcessArgs(), CancellationToken.None));
            
            Assert.That(this.logger.CallsLog.Count, Is.EqualTo(0));
        }

        [Test]
        public void should_log_Exceptions()
        {
            this.handler.Setup(h =>
                    h.ExportDataAsync(It.IsAny<DataExportProcessArgs>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            Assert.ThrowsAsync<Exception>(()
                => this.exportJob.ExecuteAsync(Create.Entity.DataExportProcessArgs(), CancellationToken.None));

            Assert.That(this.logger.CallsLog.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task should_drop_schema_on_last_retry()
        {
            var args = Create.Entity.DataExportProcessArgs();
            args.ShouldDropTenantSchema = true;

            await this.exportJob.ExecuteAsync(args, CancellationToken.None);

            this.schemaGenerator.Verify(s => s.DropTenantSchemaAsync(args.ExportSettings.Tenant.Name, CancellationToken.None), Times.Once);
        }

        private static Exception[] nonLoggableExceptions = new Exception[]
        {
            new OperationCanceledException(),
            new TaskCanceledException(),
            new PostgresException(null, null, null, "57014")
        };
    }
}
