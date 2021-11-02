using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using SQLite;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.Tests.Integration.CommandServiceTests
{
    [TestFixture]
    [NonParallelizable]
    public class CommandServiceUnitTests
    {
        private class AnyCommand : ICommand
        {
            public AnyCommand(Guid id, List<string> log)
            {
                CommandIdentifier = id;
                Log = log;
            }
            public Guid CommandIdentifier { get; }
            public List<string> Log { get; }
        }
        
        private class Aggregate : EventSourcedAggregateRoot
        {
            public void AnyCommand(AnyCommand command)
            {
                Task.Delay(500).Wait();
                command.Log.Add("command executed");
                Task.Delay(500).Wait();
            }
        }

        [OneTimeSetUp]
        public void SetupScope()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<AnyCommand>(_ => _.CommandIdentifier, (command, aggregate) =>
                {
                    aggregate.AnyCommand(command);
                });
        }
        
        [Test]
        public async Task when_waiting_for_command_execution_should_finish_waiting_after_execute_first_command() 
        {
            List<string> log = new List<string>();
            Guid aggregateId = Guid.NewGuid();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            var commandService = Abc.Create.Service.CommandService(repository: repository, aggregateLock: new AggregateLock());

            var t1 = commandService.ExecuteAsync(new AnyCommand(aggregateId, log), null, CancellationToken.None);
            log.Add("wait started");
            await commandService.WaitOnCommandAsync().ConfigureAwait(false);
            log.Add("wait finished");

            log.Should().BeEquivalentTo("wait started", "wait finished");
            
            await t1.ConfigureAwait(false);
            
            log.Should().BeEquivalentTo("wait started", "wait finished", "command executed");
        }
        
        [Test]
        public async Task when_waiting_for_command_execution_should_finish_waiting_after_first_command_received()
        {
            List<string> log = new List<string>();
            Guid aggregateId = Guid.NewGuid();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            var commandService = Abc.Create.Service.CommandService(repository: repository, aggregateLock: new AggregateLock());

            var t1 = commandService.ExecuteAsync(new AnyCommand(aggregateId, log), null, CancellationToken.None);

            log.Add("wait started");
            await commandService.WaitOnCommandAsync().ConfigureAwait(false);
            log.Add("wait finished");

            log.Should().BeEquivalentTo("wait started", "wait finished");

            await t1.ConfigureAwait(false);
            
            log.Should().BeEquivalentTo("wait started", "wait finished", "command executed");

            var t2 = commandService.ExecuteAsync(new AnyCommand(aggregateId, log), null, CancellationToken.None);

            await t2.ConfigureAwait(false);           
            
            log.Should().BeEquivalentTo("wait started", "wait finished", "command executed", "command executed");
        }
    }
}
