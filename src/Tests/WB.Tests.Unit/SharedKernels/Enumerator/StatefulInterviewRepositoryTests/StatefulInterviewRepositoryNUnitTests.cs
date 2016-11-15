using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewRepositoryTests
{
    [TestFixture]
    internal class StatefulInterviewRepositoryNUnitTests
    {
        [Test]
        public void When_StatefullInterview_has_at_least_one_LinkedOptionsChanged_event_Then_Event_store_should_no_be_updated()
        {
            var liteEventBusMock = new Mock<ILiteEventBus>();

            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: null);
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged());

            var statefulInterviewRepository = CreateStatefulInterviewRepository(statefulInterview,
                liteEventBusMock.Object);

            var result = statefulInterviewRepository.Get(Guid.NewGuid().FormatGuid());

            Assert.That(result, Is.EqualTo(statefulInterview));
            liteEventBusMock.Verify(x => x.CommitUncommittedEvents(statefulInterview, Moq.It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        [Ignore("KP-8159")]
        public void When_StatefullInterview_doesnt_have_at_least_one_LinkedOptionsChanged_event_Then_Event_store_should_no_be_updated()
        {
            var liteEventBusMock = new Mock<ILiteEventBus>();

            IQuestionnaire questionnaire = Substitute.For<IQuestionnaire>();

            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire);

            var statefulInterviewRepository = CreateStatefulInterviewRepository(statefulInterview,
                liteEventBusMock.Object);

            var result = statefulInterviewRepository.Get(Guid.NewGuid().FormatGuid());

            Assert.That(result, Is.EqualTo(statefulInterview));
            Assert.That(result.HasLinkedOptionsChangedEvents, Is.True);
            liteEventBusMock.Verify(x => x.CommitUncommittedEvents(statefulInterview, Moq.It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void When_getting_StatefullInterview_and_event_store_does_not_have_any_events_by_interview_Then_should_return_nullable_StatefullInterview()
        {
            var aggregateRootId = Guid.Parse("11111111111111111111111111111111");
            AssemblyContext.SetupServiceLocator();
            var snapshotStore = Create.Fake.SnapshotStore(aggregateRootId);
            var eventStore = Create.Fake.EventStore(aggregateRootId, Array.Empty<CommittedEvent>());
            var aggregateSnapshotter = Create.Fake.AggregateSnapshotter();
            Setup.InstanceToMockedServiceLocator(Create.AggregateRoot.StatefulInterview(questionnaireId: Guid.NewGuid(),
                userId: Guid.NewGuid(), questionnaire: null));
            var domaiRepository = Create.Service.DomainRepository(aggregateSnapshotter: aggregateSnapshotter, serviceLocator: ServiceLocator.Current);
            var aggregateRootRepository = Create.Service.EventSourcedAggregateRootRepository(snapshotStore: snapshotStore,
                eventStore: eventStore, repository: domaiRepository);

            var statefulInterviewRepository = Create.Service.StatefulInterviewRepository(aggregateRootRepository);

            var result = statefulInterviewRepository.Get(aggregateRootId.FormatGuid());

            Assert.That(result, Is.EqualTo(null));
        }

        private static StatefulInterviewRepository CreateStatefulInterviewRepository(StatefulInterview statefulInterview = null, ILiteEventBus liteEventBus = null)
            => new StatefulInterviewRepository(
                Stub<IEventSourcedAggregateRootRepository>.Returning<IEventSourcedAggregateRoot>(statefulInterview),
                liteEventBus ?? Mock.Of<ILiteEventBus>());
    }
}