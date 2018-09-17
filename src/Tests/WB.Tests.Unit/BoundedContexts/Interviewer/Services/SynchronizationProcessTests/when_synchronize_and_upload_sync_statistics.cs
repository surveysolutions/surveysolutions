using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(InterviewerOnlineSynchronizationProcess))]
    internal class when_synchronize_and_upload_sync_statistics
    {
        private Mock<IInterviewerSynchronizationService> synchronizationServiceMock;

        private readonly long downloadedBytes = 4000;
        private readonly long uploadedBytes = 6000;
        private readonly TimeSpan totalDuration = TimeSpan.FromSeconds(1);
        private Stopwatch sw;
        private Mock<IHttpStatistician> httpStatistician;

        [OneTimeSetUp]
        public async Task Context()
        {
            var principal = Setup.InterviewerPrincipal("name", "pass");

            var interviewId = Guid.NewGuid();

            var questionnaireIdentity = new QuestionnaireIdentity(
                Guid.Parse("11111111111111111111111111111111"), 1);

            var censusInterview = new InterviewView
            {
                Id = interviewId.ToString(),
                InterviewId = interviewId,
                CanBeDeleted = true,
                Status = InterviewStatus.InterviewerAssigned,
                QuestionnaireId = questionnaireIdentity.ToString()
            };

            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();
            interviewViewRepository.Store(new List<InterviewView>
            {
                censusInterview
            });

            this.synchronizationServiceMock = new Mock<IInterviewerSynchronizationService>();

            synchronizationServiceMock.Setup(x => x.GetCensusQuestionnairesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuestionnaireIdentity>(new[] { questionnaireIdentity }));
            synchronizationServiceMock.Setup(x => x.GetServerQuestionnairesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuestionnaireIdentity>(new[] { questionnaireIdentity }));
            synchronizationServiceMock.Setup(x => x.GetInterviewsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InterviewApiView>());
            synchronizationServiceMock.Setup(x => x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());
            synchronizationServiceMock.Setup(x => x.GetInterviewerAsync(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InterviewerApiView());

            this.httpStatistician = new Mock<IHttpStatistician>();

            this.httpStatistician.Setup(s => s.GetStats()).Returns(new HttpStats
                {
                    DownloadedBytes = this.downloadedBytes,
                    UploadedBytes = this.uploadedBytes,
                    Duration = this.totalDuration
                });

            IPlainStorage<InterviewerIdentity> localInterviewers = new InMemoryPlainStorage<InterviewerIdentity>();
            localInterviewers.Store(Create.Other.InterviewerIdentity());

            var viewModel = Create.Service.SynchronizationProcess(principal: principal,
                interviewersPlainStorage: localInterviewers,
                interviewViewRepository: interviewViewRepository,
                httpStatistician: httpStatistician.Object,
                interviewerSynchronizationService: synchronizationServiceMock.Object
            );

            this.sw = new Stopwatch();
            sw.Start();
            await viewModel.SynchronizeAsync(Mock.Of<IProgress<SyncProgressInfo>>(), CancellationToken.None);
            sw.Stop();
        }

        private void AssertThatSendSyncStatisticsRecieve(Expression<Func<SyncStatisticsApiView, bool>> expression)
        {
            this.synchronizationServiceMock.Verify(x =>
                x.SendSyncStatisticsAsync(It.Is(expression),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RestCredentials>()), Times.Once);
        }

        [Test]
        public void should_not_include_census_in_counter_by_new_device_interviews() => this.AssertThatSendSyncStatisticsRecieve(
            stats => stats.NewInterviewsOnDeviceCount == 0);

        [Test]
        public void should_send_download_stats() => this.AssertThatSendSyncStatisticsRecieve(
            stats => stats.TotalDownloadedBytes == this.downloadedBytes);
        
        [Test]
        public void should_send_upload_stats() => this.AssertThatSendSyncStatisticsRecieve(
            stats => stats.TotalUploadedBytes == this.uploadedBytes);
        
        [Test]
        public void should_send_totalDuration_stats() => this.AssertThatSendSyncStatisticsRecieve(
            stats => stats.TotalSyncDuration < this.sw.Elapsed && stats.TotalSyncDuration > TimeSpan.Zero);
        
        [Test]
        public void should_send_connections_speed_stats() => this.AssertThatSendSyncStatisticsRecieve(
            stats => stats.TotalConnectionSpeed == (this.uploadedBytes + this.downloadedBytes) / this.totalDuration.TotalSeconds);

        [Test]
        public void should_get_http_stats_from_httpStatistician() => this.httpStatistician.Verify(s => s.GetStats(), Times.Once);
        
        [Test]
        public void should_reset_stats_for_httpStatistician() => this.httpStatistician.Verify(s => s.Reset(), Times.Once);
    }
}
