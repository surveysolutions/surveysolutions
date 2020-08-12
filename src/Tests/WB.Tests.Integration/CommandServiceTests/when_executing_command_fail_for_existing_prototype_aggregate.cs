using System;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Services;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_command_fail_for_existing_prototype_aggregate
    {
        private class ThisCommandWillFail : ICommand { public Guid CommandIdentifier => default; }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                throw new Exception();
            }
        }

        [OneTimeSetUp] 
        public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<ThisCommandWillFail>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(repo
                => repo.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            this.promoter = new Mock<IAggregateRootPrototypePromoterService>();
            var prototype = Mock.Of<IAggregateRootPrototypeService>(p => p.GetPrototypeType(aggregateId) == PrototypeType.Temporary);

            commandService = Create.Service.CommandService(repository, promoterService: promoter.Object, prototypeService: prototype);

            BecauseOf();
        }

        private void BecauseOf() =>
            Assert.Throws<Exception>(() => commandService.Execute(new ThisCommandWillFail(), null));

        [Test]
        public void should_not_send_request_for_prototype_promotion() =>
            promoter.Verify(p => p.MaterializePrototypeIfRequired(aggregateId), Times.Never);
        
        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static Aggregate aggregateFromRepository;
        private Mock<IAggregateRootPrototypePromoterService> promoter;
    }
}
