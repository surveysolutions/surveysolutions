using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            InterviewView localInterview = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.InterviewerAssigned);
            var localInterviewStorage = Create.Storage.SqliteInmemoryStorage<InterviewView>();
            localInterviewStorage.Store(localInterview);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));
            synchronizationService
                .Setup(s => s.GetSyncInfoPackageResponse(interviewId, It.IsAny<InterviewSyncInfoPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new SyncInfoPackageResponse()));
            
            synchronizationService
                .Setup(s => s.UploadInterviewAsync(interviewId, It.IsAny<InterviewPackageApiView>(), 
                    It.IsAny<IProgress<TransferProgress>>() , It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InterviewUploadResult() { ReceivedInterviewId = interviewId }));

            var eventStore = Create.Storage.InMemorySqliteMultiFilesEventStorage();
            eventStore.Store(new UncommittedEventStream(null, new []{ 
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:1, payload: Create.Event.InterviewCreated(Guid.NewGuid(), 1)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:2, payload: Create.Event.InterviewerAssigned(Guid.NewGuid(), interviewId))
                }));


            var interviewFactory = Create.Service.InterviewerInterviewAccessor(localInterviewStorage, eventStore,
                prefilledQuestions: Create.Storage.InMemorySqlitePlainStorage<PrefilledQuestionView>());

            var principal = Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.UserId == responsibleId));

            var synchronizationStep = CreateInterviewerUploadInterviews(synchronizationService.Object,
                localInterviewStorage, interviewFactory, eventStorage: eventStore, principal: principal);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    It.IsAny<InterviewPackageApiView>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(1));
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(0));

            var hasEventsWithoutHqFlag = eventStore.HasEventsWithoutHqFlag(interviewId);
            Assert.That(hasEventsWithoutHqFlag, Is.False);

            var interviewAfterSynch = localInterviewStorage.GetById(interviewId.FormatGuid());
            Assert.That(interviewAfterSynch.CanBeDeleted, Is.False);
            Assert.That(interviewAfterSynch.FromHqSyncDateTime, Is.Not.Null);
        }

        [Test]
        public async Task when_local_interview_should_be_fully_upload()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterview = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed);
            var localInterviewStorage = Create.Storage.SqliteInmemoryStorage<InterviewView>();
            localInterviewStorage.Store(localInterview);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));
            synchronizationService
                .Setup(s => s.GetSyncInfoPackageResponse(interviewId, It.IsAny<InterviewSyncInfoPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new SyncInfoPackageResponse()));
            
            synchronizationService
                .Setup(s => s.UploadInterviewAsync(interviewId, It.IsAny<InterviewPackageApiView>(), 
                    It.IsAny<IProgress<TransferProgress>>() , It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InterviewUploadResult() { ReceivedInterviewId = interviewId }));

            var eventStore = Create.Storage.InMemorySqliteMultiFilesEventStorage();
            eventStore.Store(new UncommittedEventStream(null, new[]{
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:1, payload: Create.Event.InterviewCreated(Guid.NewGuid(), 1)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:2, payload: Create.Event.InterviewerAssigned(Guid.NewGuid(), interviewId)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:3, payload: Create.Event.InterviewCompleted())
                }));


            var interviewFactory = Create.Service.InterviewerInterviewAccessor(localInterviewStorage, eventStore,
                prefilledQuestions: Create.Storage.InMemorySqlitePlainStorage<PrefilledQuestionView>(),
                interviewMultimediaViewRepository: Create.Storage.InMemorySqlitePlainStorage<InterviewMultimediaView>());

            var principal = Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.UserId == responsibleId));

            var synchronizationStep = CreateInterviewerUploadInterviews(synchronizationService.Object,
                localInterviewStorage, interviewFactory, eventStorage: eventStore, principal: principal);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    It.IsAny<InterviewPackageApiView>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(0));
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(1));

            var eventsInDbAfterSynch = eventStore.Read(interviewId, 1);
            Assert.That(eventsInDbAfterSynch.Count(), Is.EqualTo(0));

            var interviewAfterSynch = localInterviewStorage.GetById(interviewId.FormatGuid());
            Assert.That(interviewAfterSynch, Is.Null);
        }


        [Test]
        public async Task when_local_interview_should_be_fully_upload2()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterviews = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));

            synchronizationService
                .Setup(s => s.UploadInterviewAsync(interviewId, It.IsAny<InterviewPackageApiView>(), 
                    It.IsAny<IProgress<TransferProgress>>() , It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InterviewUploadResult() { ReceivedInterviewId = interviewId }));

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s =>
                s.HasEventsWithoutHqFlag(interviewId) == true);
            var eventsContainer = Create.Entity.InterviewPackageContainer(interviewId,
                Create.Event.CommittedEvent(eventSourceId: interviewId));
            var package = new InterviewPackageApiView();
            var interviewFactory = Mock.Of<IInterviewerInterviewAccessor>(f =>
                f.GetInterviewEventStreamContainer(interviewId, false, It.IsAny<SyncInfoPackageResponse>()) == eventsContainer &&
                f.GetInterviewEventsPackageOrNull(eventsContainer, It.IsAny<SyncInfoPackageResponse>()) == package);

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
        public async Task when_locally_interview_has_new_events()
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
                f.GetInterviewEventStreamContainer(interviewId, false, It.IsAny<SyncInfoPackageResponse>()) == eventsContainer &&
                f.GetInterviewEventsPackageOrNull(eventsContainer, It.IsAny<SyncInfoPackageResponse>()) == package);

            var synchronizationStep = CreateInterviewerUploadInterviews(responsibleId, localInterviews, synchronizationService.Object, eventStore, interviewFactory);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                package,
                It.IsAny<IProgress<TransferProgress>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task when_locally_interview_has_not_new_events()
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
            interviewFactory.Verify(f => f.GetInterviewEventStreamContainer(interviewId, It.IsAny<bool>(), It.IsAny<SyncInfoPackageResponse>()), Times.Never);
        }
        
        [Test]
        public async Task when_local_interview_has_flag_about_sync_but_server_unknown_it_should_upload_all_stream()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterview = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed);
            var localInterviewStorage = Create.Storage.SqliteInmemoryStorage<InterviewView>();
            localInterviewStorage.Store(localInterview);

            var eventStore = Create.Storage.InMemorySqliteMultiFilesEventStorage();
            eventStore.Store(new UncommittedEventStream(null, new []{ 
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:1, payload: Create.Event.InterviewCreated(Guid.NewGuid(), 1)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:2, payload: Create.Event.InterviewerAssigned(Guid.NewGuid(), interviewId)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:3, payload: Create.Event.InterviewCompleted()),
            }));
            eventStore.MarkAllEventsAsReceivedByHq(interviewId);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));
            synchronizationService
                .Setup(s => s.GetSyncInfoPackageResponse(interviewId, It.IsAny<InterviewSyncInfoPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new SyncInfoPackageResponse() { HasInterview = false }));

            synchronizationService
                .Setup(s => s.UploadInterviewAsync(interviewId, It.IsAny<InterviewPackageApiView>(), 
                    It.IsAny<IProgress<TransferProgress>>() , It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InterviewUploadResult() { ReceivedInterviewId = interviewId }));

            var interviewFactory = Create.Service.InterviewerInterviewAccessor(localInterviewStorage, eventStore,
                prefilledQuestions: Create.Storage.InMemorySqlitePlainStorage<PrefilledQuestionView>(),
                interviewMultimediaViewRepository: Stub<IPlainStorage<InterviewMultimediaView>>.WithNotEmptyValues);

            var principal = Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.UserId == responsibleId));

            var synchronizationStep = CreateInterviewerUploadInterviews(synchronizationService.Object,
                localInterviewStorage, interviewFactory, eventStorage: eventStore, principal: principal);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    It.Is<InterviewPackageApiView>(p => p.FullEventStreamRequested == false),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(0));
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(1));

            var hasEventsWithoutHqFlag = eventStore.HasEventsWithoutHqFlag(interviewId);
            Assert.That(hasEventsWithoutHqFlag, Is.False);

            var interviewAfterSynch = localInterviewStorage.GetById(interviewId.FormatGuid());
            Assert.That(interviewAfterSynch, Is.Null);
        }

        [Test]
        public async Task when_local_interview_has_flag_about_sync_but_server__return_request_on_all_stream_should_upload_all_stream()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterview = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed);
            var localInterviewStorage = Create.Storage.SqliteInmemoryStorage<InterviewView>();
            localInterviewStorage.Store(localInterview);

            var eventStore = Create.Storage.InMemorySqliteMultiFilesEventStorage();
            eventStore.Store(new UncommittedEventStream(null, new []{ 
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:1, payload: Create.Event.InterviewCreated(Guid.NewGuid(), 1)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:2, payload: Create.Event.InterviewerAssigned(Guid.NewGuid(), interviewId)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:3, payload: Create.Event.InterviewCompleted()),
            }));
            eventStore.MarkAllEventsAsReceivedByHq(interviewId);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));
            synchronizationService
                .Setup(s => s.GetSyncInfoPackageResponse(interviewId, It.IsAny<InterviewSyncInfoPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new SyncInfoPackageResponse() { HasInterview = true, NeedSendFullStream = true }));
            
            synchronizationService
                .Setup(s => s.UploadInterviewAsync(interviewId, It.IsAny<InterviewPackageApiView>(), 
                    It.IsAny<IProgress<TransferProgress>>() , It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InterviewUploadResult() { ReceivedInterviewId = interviewId }));

            var interviewFactory = Create.Service.InterviewerInterviewAccessor(localInterviewStorage, eventStore,
                prefilledQuestions: Create.Storage.InMemorySqlitePlainStorage<PrefilledQuestionView>(),
                interviewMultimediaViewRepository: Stub<IPlainStorage<InterviewMultimediaView>>.WithNotEmptyValues);

            var principal = Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.UserId == responsibleId));

            var synchronizationStep = CreateInterviewerUploadInterviews(synchronizationService.Object,
                localInterviewStorage, interviewFactory, eventStorage: eventStore, principal: principal);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    It.Is<InterviewPackageApiView>(p => p.FullEventStreamRequested == true),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(0));
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(1));

            var hasEventsWithoutHqFlag = eventStore.HasEventsWithoutHqFlag(interviewId);
            Assert.That(hasEventsWithoutHqFlag, Is.False);

            var interviewAfterSynch = localInterviewStorage.GetById(interviewId.FormatGuid());
            Assert.That(interviewAfterSynch, Is.Null);
        }

        [Test]
        public async Task when_local_interview_has_flag_about_sync_should_do_not_upload_any_events()
        {
            var interviewId = Id.g1;
            var responsibleId = Id.g2;

            InterviewView localInterview = Create.Entity.InterviewView(interviewId: interviewId, status: InterviewStatus.Completed);
            var localInterviewStorage = Create.Storage.SqliteInmemoryStorage<InterviewView>();
            localInterviewStorage.Store(localInterview);

            var eventStore = Create.Storage.InMemorySqliteMultiFilesEventStorage();
            eventStore.Store(new UncommittedEventStream(null, new []{ 
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:1, payload: Create.Event.InterviewCreated(Guid.NewGuid(), 1)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:2, payload: Create.Event.InterviewerAssigned(Guid.NewGuid(), interviewId)),
                Create.Event.UncommittedEvent(eventSourceId: interviewId, eventSequence:3, payload: Create.Event.InterviewCompleted()),
            }));
            eventStore.MarkAllEventsAsReceivedByHq(interviewId);

            InterviewUploadState remoteInterviewUploadState = Create.Entity.InterviewUploadState(responsibleId, true);
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService
                .Setup(s => s.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteInterviewUploadState));
            synchronizationService
                .Setup(s => s.GetSyncInfoPackageResponse(interviewId, It.IsAny<InterviewSyncInfoPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new SyncInfoPackageResponse() { HasInterview = true, NeedSendFullStream = false }));

            var interviewFactory = Create.Service.InterviewerInterviewAccessor(localInterviewStorage, eventStore,
                prefilledQuestions: Create.Storage.InMemorySqlitePlainStorage<PrefilledQuestionView>(),
                interviewMultimediaViewRepository: Stub<IPlainStorage<InterviewMultimediaView>>.WithNotEmptyValues);

            var principal = Mock.Of<IPrincipal>(p => p.CurrentUserIdentity == Mock.Of<IUserIdentity>(i => i.UserId == responsibleId));

            var synchronizationStep = CreateInterviewerUploadInterviews(synchronizationService.Object,
                localInterviewStorage, interviewFactory, eventStorage: eventStore, principal: principal);

            await synchronizationStep.ExecuteAsync();

            synchronizationService.Verify(s => s.UploadInterviewAsync(interviewId,
                    It.IsAny<InterviewPackageApiView>(),
                    It.IsAny<IProgress<TransferProgress>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount, Is.EqualTo(0));
            Assert.That(synchronizationStep.Context.Statistics.SuccessfullyUploadedInterviewsCount, Is.EqualTo(1));

            var hasEventsWithoutHqFlag = eventStore.HasEventsWithoutHqFlag(interviewId);
            Assert.That(hasEventsWithoutHqFlag, Is.False);

            var interviewAfterSynch = localInterviewStorage.GetById(interviewId.FormatGuid());
            Assert.That(interviewAfterSynch, Is.Null);
        }



        private SynchronizationStep CreateInterviewerUploadInterviews(
            Guid responsibleId,
            InterviewView localInterview,
            ISynchronizationService synchronizationService,
            IEnumeratorEventStorage eventStore = null,
            IInterviewerInterviewAccessor interviewFactory = null)
        {
            var localStorage = Mock.Of<IPlainStorage<InterviewView>>(s =>
                s.Where(It.IsAny<Expression<Func<InterviewView, bool>>>()) == new[] {localInterview}.ToReadOnlyCollection() &&
                s.GetById(localInterview.Id) == localInterview);

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
