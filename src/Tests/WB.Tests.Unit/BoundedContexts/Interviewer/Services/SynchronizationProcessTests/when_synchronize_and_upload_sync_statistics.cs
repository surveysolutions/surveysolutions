﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [NUnit.Framework.TestOf(typeof(SynchronizationProcess))]
    internal class when_synchronize_and_upload_sync_statistics
    {
        private Mock<ISynchronizationService> synchronizationServiceMock;

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

            this.synchronizationServiceMock = new Mock<ISynchronizationService>();

            synchronizationServiceMock.Setup(x => x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuestionnaireIdentity>(new[] { questionnaireIdentity }));
            synchronizationServiceMock.Setup(x => x.GetServerQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuestionnaireIdentity>(new[] { questionnaireIdentity }));
            synchronizationServiceMock.Setup(x => x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InterviewApiView>());
            synchronizationServiceMock.Setup(x =>
                    x.CheckObsoleteInterviewsAsync(It.IsAny<List<ObsoletePackageCheck>>(), CancellationToken.None))
                .ReturnsAsync(new List<Guid>());

            var interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>(new[] { questionnaireIdentity })
                     && x.GetAllQuestionnaireIdentities() == new List<QuestionnaireIdentity>(new[] { questionnaireIdentity })
            );

            this.httpStatistician = new Mock<IHttpStatistician>();

            this.httpStatistician.Setup(s => s.GetStats()).Returns(new HttpStats
                {
                    DownloadedBytes = this.downloadedBytes,
                    UploadedBytes = this.uploadedBytes,
                    Duration = this.totalDuration
                });

            var mockOFInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();

            var viewModel = Create.Service.SynchronizationProcess(principal: principal,
                interviewViewRepository: interviewViewRepository,
                synchronizationService: synchronizationServiceMock.Object,
                questionnaireFactory: interviewerQuestionnaireAccessor,
                interviewFactory: mockOFInterviewAccessor.Object,
                httpStatistician: httpStatistician.Object
            );

            this.sw = new Stopwatch();
            sw.Start();
            await viewModel.SyncronizeAsync(Mock.Of<IProgress<SyncProgressInfo>>(), CancellationToken.None);
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
