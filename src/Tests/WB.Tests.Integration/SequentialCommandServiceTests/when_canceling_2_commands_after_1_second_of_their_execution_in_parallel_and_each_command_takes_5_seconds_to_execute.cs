using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.CommandBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.SequentialCommandServiceTests
{
    internal class when_canceling_2_commands_after_1_second_of_their_execution_in_parallel_and_each_command_takes_5_seconds_to_execute
    {
        private class StoreNameFor5Seconds : ICommand
        {
            public StoreNameFor5Seconds(string name)
            {
                this.Name = name;
            }

            public Guid CommandIdentifier { get; private set; }
            public string Name { get; private set; }
        }

        private class Aggregate : AggregateRoot
        {
            public void StoreNameFor5Seconds(StoreNameFor5Seconds command)
            {
                Task.Delay(5000).Wait();
                executedCommands.Add(command.Name);
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<StoreNameFor5Seconds>(_ => aggregateId, aggregate => aggregate.StoreNameFor5Seconds);

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = Create.SequentialCommandService(repository: repository);
        };

        Because of = () =>
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
        };

        It should_execute_only_first_command = () =>
            executedCommands.ShouldContainOnly("first");

        private static List<string> executedCommands = new List<string>();
        private static SequentialCommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
    }
}