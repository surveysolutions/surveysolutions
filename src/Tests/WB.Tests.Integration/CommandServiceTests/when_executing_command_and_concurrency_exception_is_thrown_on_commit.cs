using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.CommandServiceTests
{
    // [NonParallelizable] is required because CommandRegistry is a global static registry shared across tests.
    [TestFixture]
    [NonParallelizable]
    public class when_executing_command_and_concurrency_exception_is_thrown_on_commit
    {
        private class ConcurrencyTestUpdateCommand : ICommand
        {
            public ConcurrencyTestUpdateCommand(Guid id) => CommandIdentifier = id;
            public Guid CommandIdentifier { get; }
        }

        private class ConcurrencyTestUpdated : IEvent { }

        private class ConcurrencyTestAggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                this.ApplyEvent(new ConcurrencyTestUpdated());
            }
        }

        private static Guid aggregateId;
        private static int commitAttempts;
        private static CommandService commandService;
        private static Exception thrownException;

        [OneTimeSetUp]
        public void Context()
        {
            aggregateId = Guid.NewGuid();

            CommandRegistry
                .Setup<ConcurrencyTestAggregate>()
                .Handles<ConcurrencyTestUpdateCommand>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            commitAttempts = 0;

            var aggregate = new ConcurrencyTestAggregate();
            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_ =>
                _.GetLatest(typeof(ConcurrencyTestAggregate), aggregateId) == aggregate);

            var eventBusMock = new Mock<ILiteEventBus>();
            eventBusMock
                .Setup(bus => bus.CommitUncommittedEvents(It.IsAny<IEventSourcedAggregateRoot>(), It.IsAny<string>()))
                .Returns((IEventSourcedAggregateRoot ar, string origin) =>
                {
                    commitAttempts++;
                    if (commitAttempts == 1)
                    {
                        throw new AggregateConcurrencyException(
                            $"Unexpected stream version. Expected 1. EventSourceId: {aggregateId}");
                    }

                    return new CommittedEventStream(ar.EventSourceId, new List<CommittedEvent>());
                });

            commandService = Create.Service.CommandService(repository: repository, eventBus: eventBusMock.Object);

            thrownException = null;
            try
            {
                commandService.Execute(new ConcurrencyTestUpdateCommand(aggregateId), null);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
        }

        [Test]
        public void should_not_throw_exception()
        {
            thrownException.Should().BeNull();
        }

        [Test]
        public void should_retry_commit_after_concurrency_exception()
        {
            commitAttempts.Should().Be(2);
        }
    }
}
