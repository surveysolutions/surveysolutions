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
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    [Ignore("Flacky in GhA")]
    internal class when_canceling_2_commands_after_1_second_of_their_execution_in_parallel_and_each_command_takes_5_seconds_to_execute
    {
        private static readonly object LockObject = new object();

        private class SaveNameFor5Seconds : ICommand
        {
            public Task IsExecuted { get; }

            public SaveNameFor5Seconds(Task isExecuted, string name, Guid rootId)
            {
                this.IsExecuted = isExecuted;
                this.Name = name;
                this.AggregateId = rootId;
            }

            public Guid CommandIdentifier { get; }
            public string Name { get; }
            public Guid AggregateId { get; } 
            
            public TaskCompletionSource<bool> IsStarted { get; } = new TaskCompletionSource<bool>();

        }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void SaveNameFor5Seconds(SaveNameFor5Seconds command)
            {
                command.IsStarted.SetResult(true);
                command.IsExecuted.Wait();
                lock (LockObject)
                {
                    executedCommands.Add(command.Name);
                }
            }
        }

        [Test]
        public async Task should_execute_all_2_commands_for_2_different_aggregate_root()
        {
            // arrange 
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<SaveNameFor5Seconds>(command => command.AggregateId, aggregate => aggregate.SaveNameFor5Seconds);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), Moq.It.IsAny<Guid>()) == new Aggregate());

            commandService = Create.Service.CommandService(repository: repository, aggregateLock: new AggregateLock());

            // act
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var t1Source = new TaskCompletionSource<object>();
                var c1 = new SaveNameFor5Seconds(t1Source.Task, "first", Id.g1);
                var t1 = commandService.ExecuteAsync(c1, null, cancellationTokenSource.Token);

                var t2Source = new TaskCompletionSource<object>();
                var c2 = new SaveNameFor5Seconds(t2Source.Task, "second", Id.g2);
                var t2 = commandService.ExecuteAsync(c2, null, cancellationTokenSource.Token);

                await Task.WhenAny(c1.IsStarted.Task, c2.IsStarted.Task); // wait for both command to start execution
                await Task.Delay(100);

                cancellationTokenSource.Cancel();

                // Complete both commands execution
                t1Source.SetResult(new object());
                t2Source.SetResult(new object());

                // Wait for both commands to complete
                await Task.WhenAll(t1, t2);
            }
            catch (AggregateException) { }

            // Assert
            lock (LockObject)
            {
                Assert.That(executedCommands, Is.EquivalentTo(new []{"first", "second"}));
            }
        }

        private static readonly List<string> executedCommands = new List<string>();
        private static CommandService commandService;
    }
}
