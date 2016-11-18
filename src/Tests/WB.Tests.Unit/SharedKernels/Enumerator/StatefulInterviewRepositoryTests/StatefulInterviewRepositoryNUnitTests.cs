using System;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewRepositoryTests
{
    [TestFixture]
    internal class StatefulInterviewRepositoryNUnitTests
    {
        [Test]
        public void When_getting_StatefullInterview_and_event_store_does_not_have_any_events_by_interview_Then_should_return_nullable_StatefullInterview()
        {
            var aggregateRootId = Guid.Parse("11111111111111111111111111111111");
            AssemblyContext.SetupServiceLocator();
            var snapshotStore = Create.Fake.SnapshotStore(aggregateRootId);
            var eventStore = Create.Fake.EventStore(aggregateRootId, Array.Empty<CommittedEvent>());
            var aggregateSnapshotter = Create.Fake.AggregateSnapshotter();
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.StaticText()));
            Setup.InstanceToMockedServiceLocator(Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire, shouldBeInitialized: false));
            var domaiRepository = Create.Service.DomainRepository(aggregateSnapshotter: aggregateSnapshotter, serviceLocator: ServiceLocator.Current);
            var aggregateRootRepository = Create.Service.EventSourcedAggregateRootRepository(snapshotStore: snapshotStore,
                eventStore: eventStore, repository: domaiRepository);

            var statefulInterviewRepository = Create.Service.StatefulInterviewRepository(aggregateRootRepository);

            var result = statefulInterviewRepository.Get(aggregateRootId.FormatGuid());

            Assert.That(result, Is.EqualTo(null));
        }
    }
}