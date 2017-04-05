using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Platform.Core;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Abc.TestFactories
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

            public bool RequestMainThreadAction(Action action)
            {
                action();
                return true;
            }

            public bool ShowViewModel(MvxViewModelRequest request)
            {
                this.Requests.Add(request);
                return true;
            }

            public bool ChangePresentation(MvxPresentationHint hint)
            {
                this.Hints.Add(hint);
                return true;
            }
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