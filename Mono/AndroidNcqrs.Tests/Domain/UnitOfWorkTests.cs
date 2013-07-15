using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using Moq;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    public class UnitOfWorkTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }
        [Test]
        public void Accepting_unit_of_work_stores_and_publishes_the_events()
        {
            var commandId = Guid.NewGuid();
            var store = new Mock<IEventStore>();

            var bus = new Mock<IEventBus>();

            var domainRepository = new Mock<IDomainRepository>();

            var snapshotStore = new Mock<ISnapshotStore>();

            var snapshottingPolicy = new NoSnapshottingPolicy();

            store.Setup(s => s.Store(It.IsAny<UncommittedEventStream>()));

			bus.Setup(b => b.Publish(It.IsAny<IEnumerable<IPublishableEvent>>()));

            var sut = new UnitOfWork(commandId, 
				domainRepository.Object, 
				store.Object, 
				snapshotStore.Object, 
				bus.Object, 
				snapshottingPolicy);

            sut.Accept();

            bus.VerifyAll();
            store.VerifyAll();
        }
    }
}