using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Abc.TestFactories
{
    internal class FakeFactory
    {
        internal class MemoryStreamWithDisposeCallback : MemoryStream
        {
            private readonly Action<byte[]> callback;

            public MemoryStreamWithDisposeCallback(Action<byte[]> callback)
            {
                this.callback = callback;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && this.CanRead) callback(this.ToArray());
                base.Dispose(disposing);
            }
        }

        public MemoryStreamWithDisposeCallback MemoryStreamWithCallback(Action<byte[]> disposeCallback) 
            => new MemoryStreamWithDisposeCallback(disposeCallback);

        public IAggregateSnapshotter AggregateSnapshotter(EventSourcedAggregateRoot aggregateRoot = null, bool isARLoadedFromSnapshotSuccessfully = false)
            => Mock.Of<IAggregateSnapshotter>(_
                => _.TryLoadFromSnapshot(It.IsAny<Type>(), It.IsAny<Snapshot>(), It.IsAny<CommittedEventStream>(), out aggregateRoot) == isARLoadedFromSnapshotSuccessfully);

        public IEventStore EventStore(Guid eventSourceId, IEnumerable<CommittedEvent> committedEvents)
            => Mock.Of<IEventStore>(_ =>
                _.Read(eventSourceId, It.IsAny<int>()) == new CommittedEventStream(eventSourceId, committedEvents));

        public IPublishableEvent PublishableEvent(Guid? eventSourceId = null, IEvent payload = null)
            => Mock.Of<IPublishableEvent>(_
                => _.Payload == (payload ?? Mock.Of<IEvent>())
                && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));

        public IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionnaire = null, long? questionnaireVersion = null)
        {
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>(x => x.IsUsingExpressionStorage() == true && x.GetExpressionsPlayOrder() == new List<Guid>());

            return Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        }

        public IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(QuestionnaireDocument questionnaire)
        {
            var repository = new Mock<IQuestionnaireStorage>();
            IQuestionnaire plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);
            repository.SetReturnsDefault(plainQuestionnaire);
            repository.SetReturnsDefault(questionnaire);
            return repository.Object;
        }

        public IQuestionnaireStorage QuestionnaireRepository(KeyValuePair<string, QuestionnaireDocument>[] questionnairesWithTranslations)
        {
            var questionnairesStorage = new Mock<IQuestionnaireStorage>();

            foreach (var questionnaire in questionnairesWithTranslations)
            {
                IQuestionnaire plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire.Value);

                questionnairesStorage.Setup(repository =>
                    repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), questionnaire.Key)).Returns(plainQuestionnaire);
            }
            
            return questionnairesStorage.Object;
        }

        public ISnapshotStore SnapshotStore(Guid aggregateRootId, Snapshot snapshot = null)
            => Mock.Of<ISnapshotStore>(_
                => _.GetSnapshot(aggregateRootId, It.IsAny<int>()) == snapshot);

        public IStatefulInterviewRepository StatefulInterviewRepositoryWith(IStatefulInterview interview)
        {
            var result = Substitute.For<IStatefulInterviewRepository>();
            result.Get(null).ReturnsForAnyArgs(interview);
            return result;
        }

        public IMvxViewDispatcher MvxMainThreadDispatcher1() => new MockDispatcher();

        public class MockDispatcher: MvxMainThreadDispatcher, IMvxViewDispatcher
        {
            public readonly List<MvxViewModelRequest> Requests = new List<MvxViewModelRequest>();
            public readonly List<MvxPresentationHint> Hints = new List<MvxPresentationHint>();

            public Task<bool> ShowViewModel(MvxViewModelRequest request)
            {
                this.Requests.Add(request);
                return Task.FromResult(true);
            }

            public Task<bool> ChangePresentation(MvxPresentationHint hint)
            {
                this.Hints.Add(hint);
                return Task.FromResult(true);
            }

            public Task ExecuteOnMainThreadAsync(Action action, bool maskExceptions = true)
            {
                throw new NotImplementedException();
            }

            public Task ExecuteOnMainThreadAsync(Func<Task> action, bool maskExceptions = true)
            {
                throw new NotImplementedException();
            }

            public override bool RequestMainThreadAction(Action action, bool maskExceptions = true)
            {
                action();
                return true;
            }

            public override bool IsOnMainThread => true;
        }

        public IDataExportFileAccessor DataExportFileAccessor()
        {
            var exportFileAccessor = new Mock<IDataExportFileAccessor>();
            exportFileAccessor
                .Setup(s => s.GetExternalStoragePath(It.IsAny<string>()))
                .Returns<string>(name => $"export/" + name);
            return exportFileAccessor.Object;
        }
        

        public IMvxMainThreadDispatcher MvxMainThreadDispatcher() => new FakeMvxMainThreadDispatcher();

        private class FakeMvxMainThreadDispatcher : IMvxMainThreadDispatcher
        {
            public bool RequestMainThreadAction(Action action, bool maskExceptions = true)
            {
                action.Invoke();
                return true;
            }

            public bool IsOnMainThread => true;
        }
        

        public ServiceLocatorBuilder ServiceLocator()
        {
            return new ServiceLocatorBuilder();
        }

        internal class ServiceLocatorBuilder
        {
            private readonly Mock<IServiceLocator> mock = new Mock<IServiceLocator>();
            public IServiceLocator Object => mock.Object;

            public ServiceLocatorBuilder With<T>(T item)
            {
                mock.Setup(sl => sl.GetInstance<T>()).Returns(item);
                return this;
            }
        }

        public IPayloadProvider PayloadProvider() => new PayloadProvier();

        internal class PayloadProvier : IPayloadProvider
        {
            private static long IdSource = 0;

            public IPayload AsBytes(byte[] bytes, string endpoint)
            {
                return new Payload
                {
                    Bytes = bytes,
                    Type = PayloadType.Bytes,
                    Id = Interlocked.Increment(ref IdSource),
                    Endpoint = endpoint
                };
            }

            public IPayload AsStream(byte[] bytes, string endpoint)
            {
                return new Payload
                {
                    Stream = new MemoryStream(bytes),
                    Id = Interlocked.Increment(ref IdSource),
                    Type = PayloadType.Stream,
                    Endpoint = endpoint
                };
            }
        }

        public FakeNearbyConnection GoogleConnection() => new FakeNearbyConnection();

        internal abstract class FakeNearbyConnectionBase : INearbyConnection
        {
            public Task<NearbyStatus> StartDiscoveryAsync(string serviceName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<string> StartAdvertisingAsync(string serviceName, string name, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<NearbyStatus> RequestConnectionAsync(string name, string endpoint, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<NearbyStatus> AcceptConnectionAsync(string endpoint)
            {
                throw new NotImplementedException();
            }

            public Task<NearbyStatus> RejectConnectionAsync(string endpoint)
            {
                throw new NotImplementedException();
            }

            public virtual Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
            {
                throw new NotImplementedException();
            }

            public void StopAllEndpoint()
            {
                throw new NotImplementedException();
            }

            public IObservable<INearbyEvent> Events { get; } = new Subject<INearbyEvent>();
            public ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; } = new CovariantObservableCollection<RemoteEndpoint>();
            public void StopDiscovery()
            {
            }

            public void StopAdvertising()
            {
                throw new NotImplementedException();
            }

            public void StopAll()
            {
                throw new NotImplementedException();
            }
        }

        internal class FakeNearbyConnection : FakeNearbyConnectionBase
        {
            private readonly IDictionary<string, INearbyCommunicator> clientsMap = new Dictionary<string, INearbyCommunicator>();
            private readonly IDictionary<string, string> connectionMap = new Dictionary<string, string>();

            public FakeNearbyConnection SetConnectionManager(string endpoint, INearbyCommunicator manager)
            {
                clientsMap.Add(endpoint, manager);
                return this;
            }

            public FakeNearbyConnection MapConnection(string fromEndpoint, string toEndpoint)
            {
                connectionMap.Add(fromEndpoint, toEndpoint);
                return this;
            }

            public FakeNearbyConnection WithTwoWayClientServerConnectionMap(INearbyCommunicator server, INearbyCommunicator client)
            {
                return this.SetConnectionManager("server", server)
                    .SetConnectionManager("client", client)
                    .MapConnection("server", "client")
                    .MapConnection("client", "server");
            }

            private TimeSpan[] delays = null;

            public FakeNearbyConnection WithDelaysOnResponse(params TimeSpan[] delays)
            {
                this.delays = delays;
                return this;
            }

            public override async Task<NearbyStatus> SendPayloadAsync(string to, IPayload payload)
            {
                var from = connectionMap[to];
                var toClient = clientsMap[to];
                var fromClient = clientsMap[from];
                int delayIdx = 0;

                if (payload.Type != PayloadType.Bytes)
                {
                    // google services notify on outgoing progress
                    fromClient.RecievePayloadTransferUpdate(this, to, new NearbyPayloadTransferUpdate
                    {
                        Status = TransferStatus.InProgress,
                        BytesTransferred = 0,
                        Id = payload.Id
                    });
                }

                fromClient.RecievePayloadTransferUpdate(this, to, new NearbyPayloadTransferUpdate
                {
                    Status = TransferStatus.Success,
                    BytesTransferred = 100,
                    Id = payload.Id
                });

                await NextDelay();

                // google service notify that payload is received
                await toClient.RecievePayloadAsync(this, from, payload);

                await NextDelay();

                if (payload.Type != PayloadType.Bytes)
                {
                    toClient.RecievePayloadTransferUpdate(this, from, new NearbyPayloadTransferUpdate
                    {
                        Status = TransferStatus.InProgress,
                        BytesTransferred = 0,
                        Id = payload.Id
                    });
                }

                await NextDelay();

                toClient.RecievePayloadTransferUpdate(this, from, new NearbyPayloadTransferUpdate
                {
                    Status = TransferStatus.Success,
                    BytesTransferred = 100,
                    Id = payload.Id
                });

                async Task NextDelay()
                {
                    if (delays != null)
                    {
                        if (delayIdx < delays.Length)
                        {
                            await Task.Delay(delays[delayIdx++]);
                        }
                    }
                }

                return NearbyStatus.Ok;
            }
        }

        internal class Payload : IPayload
        {
            public string Endpoint { get; set; }
            public byte[] Bytes { get; set; }
            public long Id { get; set; }
            public Stream Stream { get; set;  }
            public PayloadType Type { get; set; }

            public Task ReadStreamAsync()
            {
                var ms = Stream as MemoryStream;
                BytesFromStream = Task.FromResult(ms.ToArray());
                return BytesFromStream;
            }

            public Task<byte[]> BytesFromStream { get; set; }
        }
    }
}
