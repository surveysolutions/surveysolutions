using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.Prototype
{
    class PromoterServiceTests
    {
        [TestCase(PrototypeType.Permanent, false)]
        [TestCase(PrototypeType.Temporary, true)]
        [TestCase(null, false)]
        public void should_only_promote_Temporary_prototypes(PrototypeType? type, bool shouldPromote)
        {
            var aggregateId = Id.g1;

            var fixture = PrepareFixture(aggregateId, type);

            fixture.GetMock<IInMemoryEventStore>(); // freeze mock for later use

            // setup
            var promoter = fixture.Create<AggregateRootPrototypePromoterService>();

            // act
            promoter.MaterializePrototypeIfRequired(aggregateId);

            fixture.GetMock<IInMemoryEventStore>()
                .Verify(b => b.Read(aggregateId, It.IsAny<int>()), () => shouldPromote ? Times.Once() : Times.Never());
        }

        [Test]
        public void should_remove_prototype_mark_on_promotion()
        {
            var aggregateId = Id.g1;

            var fixture = PrepareFixture(aggregateId);

            // setup
            var promoter = fixture.Create<AggregateRootPrototypePromoterService>();

            // act
            promoter.MaterializePrototypeIfRequired(aggregateId);

            // arrange
            fixture.GetMock<IAggregateRootPrototypeService>()
                .Verify(p => p.RemovePrototype(aggregateId), Times.Once);
        }

        [Test]
        public void should_evict_from_cache_on_promotion()
        {
            var aggregateId = Id.g1;

            var fixture = PrepareFixture(aggregateId);

            // setup
            var promoter = fixture.Create<AggregateRootPrototypePromoterService>();

            // act
            promoter.MaterializePrototypeIfRequired(aggregateId);

            // arrange
            Assert.That(fixture.Create<IAggregateRootCache>().GetAggregateRoot(aggregateId), Is.Null);
        }

        [Test]
        public void should_mark_as_prototype_on_exception()
        {
            var aggregateId = Id.g1;

            var fixture = PrepareFixture(aggregateId);
            
            fixture.GetMock<IInMemoryEventStore>()
                .Setup(r => r.Read(aggregateId, It.IsAny<int>()))
                .Throws<Exception>();

            // setup
            var promoter = fixture.Create<AggregateRootPrototypePromoterService>();

            // act
            Assert.Throws<Exception>(() => promoter.MaterializePrototypeIfRequired(aggregateId));

            fixture.GetMock<IAggregateRootPrototypeService>()
                .Verify(p => p.MarkAsPrototype(aggregateId, PrototypeType.Temporary), Times.Once);
        }

        [Test]
        public void should_store_events_on_promotion()
        {
            var aggregateId = Id.g1;

            var fixture = PrepareFixture(aggregateId);
            fixture.GetMock<ILiteEventBus>(); // freeze mock

            fixture.GetMock<IInMemoryEventStore>()
                .Setup(s => s.Read(aggregateId, 0))
                .Returns(new List<CommittedEvent>
                {
                    Create.Event.CommittedEvent("prototype", aggregateId, null, Id.gA),
                    Create.Event.CommittedEvent("prototype", aggregateId, null, Id.gB),
                    Create.Event.CommittedEvent("prototype", aggregateId, null, Id.gC),
                });

            // setup
            var promoter = fixture.Create<AggregateRootPrototypePromoterService>();
            
            // act
            promoter.MaterializePrototypeIfRequired(aggregateId);

            // verify
            fixture.GetMock<IEventStore>()
                .Verify(s => s.Store(It.Is<UncommittedEventStream>(s => 
                    s.Origin == "prototype" &&
                    s.ToList()[0].EventIdentifier == Id.gA &&
                    s.ToList()[1].EventIdentifier == Id.gB &&
                    s.ToList()[2].EventIdentifier == Id.gC)));

            fixture.GetMock<ILiteEventBus>()
                .Verify(b => b.PublishCommittedEvents(It.IsAny<CommittedEventStream>()), Times.Once);
        }

        private Fixture PrepareFixture(Guid aggregateId, PrototypeType? prototypeType = PrototypeType.Temporary)
        {
            var fixture = Create.Other.AutoFixture();

            fixture.GetMock<IAggregateRootPrototypeService>()
                .Setup(p => p.GetPrototypeType(aggregateId))
                .Returns(prototypeType);

            fixture.GetMock<IEventStore>()
                .Setup(s => s.Store(It.IsAny<UncommittedEventStream>()))
                .Returns(new CommittedEventStream(aggregateId));

            var cache = Create.Storage.NewAggregateRootCache();
            fixture.Inject<IAggregateRootCache>(cache);

            return fixture;
        }

    }
}
