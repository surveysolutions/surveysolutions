using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using MvvmCross.Platform.Core;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.TestFactories
{
    internal class FakeFactory
    {
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
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>();

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

        public ISnapshotStore SnapshotStore(Guid aggregateRootId, Snapshot snapshot = null)
            => Mock.Of<ISnapshotStore>(_
                => _.GetSnapshot(aggregateRootId, It.IsAny<int>()) == snapshot);

        public IStatefulInterviewRepository StatefulInterviewRepositoryWith(IStatefulInterview interview)
        {
            var result = Substitute.For<IStatefulInterviewRepository>();
            result.Get(null).ReturnsForAnyArgs(interview);
            return result;
        }

        public IMvxMainThreadDispatcher MvxMainThreadDispatcher() => new FakeMvxMainThreadDispatcher();

        private class FakeMvxMainThreadDispatcher : IMvxMainThreadDispatcher
        {
            public bool RequestMainThreadAction(Action action)
            {
                action.Invoke();
                return true;
            }
        }
    }
}