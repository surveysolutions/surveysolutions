using System;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Services;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_command_for_new_prototype_aggregate
    {
        private class Create : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Created : IEvent { }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                ApplyEvent(new Created());
            }
        }

        [OneTimeSetUp]
        public void context()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<Create>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            IServiceLocator serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate());

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>();

            this.promoter = new Mock<IAggregateRootPrototypePromoterService>();
            var prototype = Mock.Of<IAggregateRootPrototypeService>(p => p.GetPrototypeType(aggregateId) == PrototypeType.Temporary);

            commandService = Abc.Create.Service.CommandService(repository, 
                promoterService: promoter.Object, 
                prototypeService: prototype,
                serviceLocator: serviceLocator);

            BecauseOf();
        }

        private void BecauseOf() =>
            commandService.Execute(new Create(), null);

        [Test]
        public void should_not_send_request_for_prototype_promotion() =>
            promoter.Verify(p => p.MaterializePrototypeIfRequired(aggregateId), Times.Never);

        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static Aggregate aggregateFromRepository;
        private Mock<IAggregateRootPrototypePromoterService> promoter;
    }
}
