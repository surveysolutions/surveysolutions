using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Services;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_command_for_existing_prototype_aggregate
    {
        private class Update : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Updated : IEvent { }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                ApplyEvent(new Updated());
            }
        }

        [OneTimeSetUp] 
        public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<Update>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(repo
                => repo.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            this.promoter = new Mock<IAggregateRootPrototypePromoterService>();
            var prototype = Mock.Of<IAggregateRootPrototypeService>(p => p.GetPrototypeType(aggregateId) == PrototypeType.Temporary);

            commandService = Create.Service.CommandService(repository, promoterService: promoter.Object, prototypeService: prototype);

            BecauseOf();
        }

        private void BecauseOf() =>
            commandService.Execute(new Update(), null);

        [Test]
        public void should_send_request_for_prototype_promotion() =>
            promoter.Verify(p => p.MaterializePrototypeIfRequired(aggregateId), Times.Once);

        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static Aggregate aggregateFromRepository;
        private Mock<IAggregateRootPrototypePromoterService> promoter;
    }
}
