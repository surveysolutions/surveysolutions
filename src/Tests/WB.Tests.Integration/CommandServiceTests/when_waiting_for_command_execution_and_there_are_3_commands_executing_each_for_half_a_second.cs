using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;

namespace WB.Tests.Integration.CommandServiceTests
{
    [TestFixture]
    internal class when_waiting_for_command_execution_and_there_are_3_commands_executing_each_for_half_a_second
    {
        private class ExecuteForHalfASecond : ICommand
        {
            public ExecuteForHalfASecond() { }
            public Guid CommandIdentifier { get; private set; }
        }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void ExecuteForHalfASecond(ExecuteForHalfASecond command)
            {
                Task.Delay(500).Wait();
                log.Add("command executed");
            }
        }

        [Test]
        public async Task should_finish_waiting_after_3_commands_are_executed()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<ExecuteForHalfASecond>(_ => aggregateId, aggregate => aggregate.ExecuteForHalfASecond);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = Create.CommandService(repository: repository);

            var t1 = commandService.ExecuteAsync(new ExecuteForHalfASecond(), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t2 = commandService.ExecuteAsync(new ExecuteForHalfASecond(), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t3 = commandService.ExecuteAsync(new ExecuteForHalfASecond(), null, CancellationToken.None);
            Task.Delay(5).Wait();

            await commandService.WaitPendingCommandsAsync();

            log.Add("wait finished");

            log.ShouldContainOnly("command executed", "command executed", "command executed", "wait finished");
        }

        private static readonly List<string> log = new List<string>();
        private static CommandService commandService;
        private static readonly Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
    }
}