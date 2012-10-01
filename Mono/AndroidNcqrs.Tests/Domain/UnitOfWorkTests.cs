using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using AndroidMocks;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    public class UnitOfWorkTests
    {
        [Test]
        public void Accepting_unit_of_work_stores_and_publishes_the_events()
        {
            var commandId = Guid.NewGuid();
            var store = new DynamicMock<IEventStore>();

            var bus = new DynamicMock<IEventBus>();

            var domainRepository = new DynamicMock<IDomainRepository>();

            var snapshotStore = new DynamicMock<ISnapshotStore>();

            var snapshottingPolicy = new NoSnapshottingPolicy();

            store.Expect(s => s.Store(null));

	        bus.Expect(b => b.Publish((IEnumerable<IPublishableEvent>) null));

            var sut = new UnitOfWork(commandId, 
				domainRepository.Instance, 
				store.Instance, 
				snapshotStore.Instance, 
				bus.Instance, 
				snapshottingPolicy);

            sut.Accept();

            bus.VerifyAllExpectations();
            store.VerifyAllExpectations();
        }
    }
}