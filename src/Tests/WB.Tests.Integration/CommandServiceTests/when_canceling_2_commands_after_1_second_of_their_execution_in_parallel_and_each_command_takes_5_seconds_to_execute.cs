using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_canceling_2_commands_after_1_second_of_their_execution_in_parallel_and_each_command_takes_5_seconds_to_execute
    {
        private class SaveNameFor5Seconds : ICommand
        {
            public SaveNameFor5Seconds(string name)
            {
                this.Name = name;
            }

            public Guid CommandIdentifier { get; private set; }
            public string Name { get; private set; }
            public Guid AggregateId { get; } = Guid.NewGuid();
        }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void SaveNameFor5Seconds(SaveNameFor5Seconds command)
            {
                Task.Delay(5000).Wait();
                executedCommands.Add(command.Name);
            }
        }

        [Test]
        public void should_execute_all_2_commands()
        {
            // arrange 
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<SaveNameFor5Seconds>(command => command.AggregateId, aggregate => aggregate.SaveNameFor5Seconds);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), Moq.It.IsAny<Guid>()) == new Aggregate());

            commandService = Create.Service.CommandService(repository: repository);

            // act
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var t1 = commandService.ExecuteAsync(new SaveNameFor5Seconds("first"), null, cancellationTokenSource.Token);
                Task.Delay(5).Wait();
                var t2 = commandService.ExecuteAsync(new SaveNameFor5Seconds("second"), null, cancellationTokenSource.Token);
                Task.Delay(5).Wait();

                Task.Delay(1000).Wait();
                cancellationTokenSource.Cancel();

                Task.WaitAll(t1, t2);
            }
            catch (AggregateException) { }

            // Assert
            Assert.That(executedCommands, Is.EquivalentTo(new []{"first", "second"}));
        }

        private static readonly List<string> executedCommands = new List<string>();
        private static CommandService commandService;
    }
}