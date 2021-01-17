using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;

namespace WB.Tests.Integration.SequentialCommandServiceTests
{
    [Ignore("Flacky in GhA")]
    internal class when_canceling_2_commands_after_1_second_of_their_execution_in_parallel_and_each_command_takes_5_seconds_to_execute
    {
        private class StoreNameFor5Seconds : ICommand
        {
            public StoreNameFor5Seconds(string name)
            {
                this.Name = name;
            }

            public Guid CommandIdentifier { get; private set; }
            public string Name { get; }
            public Guid AggregateId { get; } = Guid.NewGuid();
        }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void StoreNameFor5Seconds(StoreNameFor5Seconds command)
            {
                Task.Delay(5000).Wait();
                executedCommands.Add(command.Name);
            }
        }

        [NUnit.Framework.Test]
        public void should_execute_only_first_command()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<StoreNameFor5Seconds>(command => command.AggregateId, aggregate => aggregate.StoreNameFor5Seconds);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), Moq.It.IsAny<Guid>()) == new Aggregate());

            commandService = IntegrationCreate.SequentialCommandService(repository: repository);

            BecauseOf();

            executedCommands.Should().BeEquivalentTo("first");
        }

        public void BecauseOf()
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var t1 = commandService.ExecuteAsync(new StoreNameFor5Seconds("first"), null, cancellationTokenSource.Token);
                Task.Delay(5).Wait();
                var t2 = commandService.ExecuteAsync(new StoreNameFor5Seconds("second"), null, cancellationTokenSource.Token);
                Task.Delay(5).Wait();

                Task.Delay(1000).Wait();
                cancellationTokenSource.Cancel();

                Task.WaitAll(t1, t2);
            }
            catch (AggregateException) { }
        }

        private static List<string> executedCommands = new List<string>();
        private static SequentialCommandService commandService;
    }
}
