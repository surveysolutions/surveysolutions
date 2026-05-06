using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    [TestFixture]
    [NonParallelizable]
    public class when_executing_command_and_concurrency_exception_is_thrown_on_commit
    {
        private class UpdateCommand : ICommand
        {
            public UpdateCommand(Guid id) => CommandIdentifier = id;
            public Guid CommandIdentifier { get; }
        }

        private class Updated : WB.Core.Infrastructure.EventBus.IEvent { }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                this.ApplyEvent(new Updated());
            }
        }

        private static Guid aggregateId = Guid.NewGuid();
        private static int commitAttempts;
        private static CommandService commandService;
        private static Exception thrownException;

        [OneTimeSetUp]
        public void Context()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<UpdateCommand>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            commitAttempts = 0;

            var aggregate = new Aggregate();
            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_ =>
                _.GetLatest(typeof(Aggregate), aggregateId) == aggregate);

            var eventBusMock = new Mock<ILiteEventBus>();
            eventBusMock
                .Setup(bus => bus.CommitUncommittedEvents(It.IsAny<IEventSourcedAggregateRoot>(), It.IsAny<string>()))
                .Returns((IEventSourcedAggregateRoot ar, string origin) =>
                {
                    commitAttempts++;
                    if (commitAttempts == 1)
                    {
                        throw new InvalidOperationException(
                            $"Unexpected stream version. Expected 1. EventSourceId: {aggregateId}");
                    }

                    return new CommittedEventStream(ar.EventSourceId, new List<CommittedEvent>());
                });

            commandService = Create.Service.CommandService(repository: repository, eventBus: eventBusMock.Object);

            thrownException = null;
            try
            {
                commandService.Execute(new UpdateCommand(aggregateId), null);
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
