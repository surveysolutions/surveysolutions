using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using Rhino.Mocks;
using MockRepository = Rhino.Mocks.MockRepository;

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
            var store = MockRepository.GenerateMock<IEventStore>();
            var bus = MockRepository.GenerateMock<IEventBus>();
            var domainRepository = MockRepository.GenerateMock<IDomainRepository>();
            var snapshotStore = MockRepository.GenerateMock<ISnapshotStore>();
            var snapshottingPolicy = new NoSnapshottingPolicy();

            store.Expect(s => s.Store(null)).IgnoreArguments();
            bus.Expect(b => b.Publish((IEnumerable<IPublishableEvent>) null)).IgnoreArguments();

            var sut = new UnitOfWork(commandId, domainRepository, store, snapshotStore, bus, snapshottingPolicy);
            sut.Accept();

            bus.VerifyAllExpectations();
            store.VerifyAllExpectations();
        }
    }
}