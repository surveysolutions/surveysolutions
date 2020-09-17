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
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    public class when_executing_command_on_event_sources_ar_having_postprocessor
    {
        public class CommandWithPostProcessor : ICommand
        {
            public Guid CommandIdentifier { get; private set; }
        }

        public class Updated : IEvent
        {
        }

        public class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }
            public void Handle(CommandWithPostProcessor commandWithPostProcessor)
            {
                ApplyEvent(new Updated());
            }
        }

        public class PostProcessor : ICommandPostProcessor<Aggregate, CommandWithPostProcessor>
        {
            public virtual void Process(Aggregate aggregate, CommandWithPostProcessor commandWithPostProcessor)
            {
            }
        }

        [OneTimeSetUp]
        public void context()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<CommandWithPostProcessor>(_ => aggregateId, 
                    (command, aggregate) => aggregate.Handle(command), 
                    config => config.PostProcessBy<PostProcessor>());

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(repo
                => repo.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            this.promoter = new Mock<IAggregateRootPrototypePromoterService>();
            this.promoter
                .Setup(x => x.MaterializePrototypeIfRequired(aggregateId))
                .Callback(() => { materialized = true; });

            this.mockOfPostProcessor = new Mock<PostProcessor>();
            this.mockOfPostProcessor
                .Setup(x => x.Process(It.IsAny<Aggregate>(), It.IsAny<CommandWithPostProcessor>()))
                .Callback(() => { wasMaterializedOnPostProcessing = materialized; });

            var prototype =
                Mock.Of<IAggregateRootPrototypeService>(p =>
                    p.GetPrototypeType(aggregateId) == PrototypeType.Temporary);

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(PostProcessor)) == mockOfPostProcessor.Object);

            commandService = Create.Service.CommandService(repository, promoterService: promoter.Object,
                prototypeService: prototype, serviceLocator: serviceLocator);
            
            BecauseOf();
        }
        private void BecauseOf() =>
            commandService.Execute(new CommandWithPostProcessor(), null);

        [Test]
        public void should_send_request_for_prototype_promotion() =>
            promoter.Verify(p => p.MaterializePrototypeIfRequired(aggregateId), Times.Once);

        [Test]
        public void should_call_process_method_in_command_post_processor() =>
            mockOfPostProcessor.Verify(x => x.Process(It.IsAny<Aggregate>(), It.IsAny<CommandWithPostProcessor>()), Times.Once);

        [Test]
        public void should_have_materialized_ar_in_command_post_processor() =>
            Assert.That(wasMaterializedOnPostProcessing, Is.True);

        private Mock<PostProcessor> mockOfPostProcessor;
        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static Aggregate aggregateFromRepository;
        private Mock<IAggregateRootPrototypePromoterService> promoter;
        private bool materialized = false;
        private bool wasMaterializedOnPostProcessing = false;
    }
}
