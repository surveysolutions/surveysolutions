using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Services;
using WB.Services.Scheduler.Services.Implementation.HostedServices;

namespace WB.Services.Scheduler.Tests.ProgressReporter
{
    class JobProgressReporterTests : with_service_collection
    {
        [Test]
        public async Task should_continue_report_job_progress_if_jobWriter_fails()
        {
            var serviceCollection = NewServiceCollection();
            serviceCollection.AddTransient<JobProgressReporterBackgroundService, JobProgressReporterBackgroundService>();

            var writerMock = new Mock<IJobProgressReportWriter>();
            writerMock
                .Setup(w => w.WriteReportAsync(It.IsAny<IJobEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("this is error"));

            serviceCollection.AddTransient(s => writerMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var reporter = serviceProvider.GetService<JobProgressReporterBackgroundService>();
            var cts = new CancellationTokenSource();

            await reporter.StartAsync(cts.Token);

            // act
            
            // add 3 jobs to execute
            reporter.UpdateJobData(1, "1", "1");
            reporter.UpdateJobData(1, "1", "1");
            reporter.UpdateJobData(1, "1", "1");

            await reporter.AbortAsync(cts.Token);

            // assert
            writerMock.Verify(w => w.WriteReportAsync(It.IsAny<IJobEvent>(), It.IsAny<CancellationToken>()), 
                Times.Exactly(3),
                "should be called three times, even if every execution throws an error");
        }
    }
}
