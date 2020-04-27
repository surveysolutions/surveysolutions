﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.SynchronizationSteeps
{
    [TestFixture]
    public class InterviewerUploadInterviewsTests
    {
        [Test]
        public async Task when_local_interview_should_be_partial_upload()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterviews = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.InterviewerAssigned);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s =>
                s.HasEventsWithoutHqFlag(interviewId) == true);
            var eventsContainer = Create.Entity.InterviewPackageContainer(interviewId,
                Create.Event.CommittedEvent(eventSourceId: interviewId));
            var package = new InterviewPackageApiView();
            var interviewFactory = Mock.Of<IInterviewerInterviewAccessor>(f =>
                f.GetInterviewEventStreamContainer(interviewId, false) == eventsContainer &&
                f.GetInterviewEventsPackageOrNull(eventsContainer) == package);

            var synchronizationStep = CreateInterviewerUploadInterviews(responsibleId, localInterviews, synchronizationService.Object, eventStore, interviewFactory);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    package,
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Mock.Get(interviewFactory).Verify(f => f.MarkEventsAsReceivedByHQ(interviewId), Times.Once);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(1));

            Mock.Get(interviewFactory).Verify(f => f.RemoveInterview(interviewId), Times.Never);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(0));
        }


        [Test]
        public async Task when_local_interview_should_be_fully_upload()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterviews = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s =>
                s.HasEventsWithoutHqFlag(interviewId) == true);
            var eventsContainer = Create.Entity.InterviewPackageContainer(interviewId,
                Create.Event.CommittedEvent(eventSourceId: interviewId));
            var package = new InterviewPackageApiView();
            var interviewFactory = Mock.Of<IInterviewerInterviewAccessor>(f =>
                f.GetInterviewEventStreamContainer(interviewId, false) == eventsContainer &&
                f.GetInterviewEventsPackageOrNull(eventsContainer) == package);

            var synchronizationStep = CreateInterviewerUploadInterviews(responsibleId, localInterviews, synchronizationService.Object, eventStore, interviewFactory);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    package,
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Mock.Get(interviewFactory).Verify(f => f.MarkEventsAsReceivedByHQ(interviewId), Times.Never);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(0));

            Mock.Get(interviewFactory).Verify(f => f.RemoveInterview(interviewId), Times.Once);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(1));
        }


        [Test]
        public async Task when_localy_interview_has_new_events()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterviews = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.InterviewerAssigned);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s =>
                s.HasEventsWithoutHqFlag(interviewId) == true);
            var eventsContainer = Create.Entity.InterviewPackageContainer(interviewId, 
                Create.Event.CommittedEvent(eventSourceId: interviewId));
            var package = new InterviewPackageApiView();
            var interviewFactory = Mock.Of<IInterviewerInterviewAccessor>(f =>
                f.GetInterviewEventStreamContainer(interviewId, false) == eventsContainer &&
                f.GetInterviewEventsPackageOrNull(eventsContainer) == package);

            var synchronizationStep = CreateInterviewerUploadInterviews(responsibleId, localInterviews, synchronizationService.Object, eventStore, interviewFactory);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                package,
                It.IsAny<IProgress<TransferProgress>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task when_localy_interview_has_not_new_events()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterviews = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.InterviewerAssigned);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s =>
                s.HasEventsWithoutHqFlag(interviewId) == false);
            var interviewFactory = new Mock<IInterviewerInterviewAccessor>();

            var synchronizationStep = CreateInterviewerUploadInterviews(responsibleId, localInterviews, synchronizationService.Object, eventStore, interviewFactory.Object);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    It.IsAny<InterviewPackageApiView>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            interviewFactory.Verify(f => f.GetInterviewEventStreamContainer(interviewId, It.IsAny<bool>()), Times.Never);
        }

        private SynchronizationStep CreateInterviewerUploadInterviews(
            Guid responsibleId,
            InterviewView localInterview,
            ISynchronizationService synchronizationService,
            IEnumeratorEventStorage eventStore = null,
            IInterviewerInterviewAccessor interviewFactory = null)
        {
            var localStorage = new SqliteInmemoryStorage<InterviewView>();
            localStorage.Store(localInterview);

            var principal = Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.UserId == responsibleId));

            return CreateInterviewerUploadInterviews(
                synchronizationService,
                localStorage,
                eventStorage: eventStore,
                interviewFactory: interviewFactory,
                principal: principal);
        }

        private SynchronizationStep CreateInterviewerUploadInterviews(
            ISynchronizationService synchronizationService = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IInterviewerInterviewAccessor interviewFactory = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IImageFileStorage imagesStorage = null,
            IAudioFileStorage audioFileStorage = null,
            IAudioAuditFileStorage audioAuditFileStorage = null,
            IInterviewerSettings interviewerSettings = null,
            IEnumeratorEventStorage eventStorage = null,
            IPrincipal principal = null)
        {
            var downloadHqChangesForInterview = new InterviewerUploadInterviews(
                interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                interviewMultimediaViewStorage ?? new InMemoryPlainStorage<InterviewMultimediaView>(Mock.Of<ILogger>()),
                Mock.Of<ILogger>(),
                imagesStorage ?? Mock.Of<IImageFileStorage>(),
                audioFileStorage ?? Mock.Of<IAudioFileStorage>(s => s.GetBinaryFilesForInterview(It.IsAny<Guid>()) == Task.FromResult(new List<InterviewBinaryDataDescriptor>())),
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                audioAuditFileStorage ?? Mock.Of<IAudioAuditFileStorage>(s => s.GetBinaryFilesForInterview(It.IsAny<Guid>()) == Task.FromResult(new List<InterviewBinaryDataDescriptor>())),
                0,
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                interviewerSettings ?? Mock.Of<IInterviewerSettings>(s => s.PartialSynchronizationEnabled == true && s.AllowSyncWithHq == true),
                eventStorage ?? Mock.Of<IEnumeratorEventStorage>(),
                principal ?? Mock.Of<IPrincipal>());

            downloadHqChangesForInterview.Context = new EnumeratorSynchonizationContext();
            downloadHqChangesForInterview.Context.Progress = new Progress<SyncProgressInfo>();
            downloadHqChangesForInterview.Context.Statistics = new SynchronizationStatistics();

            return downloadHqChangesForInterview;
        }
    }
}
