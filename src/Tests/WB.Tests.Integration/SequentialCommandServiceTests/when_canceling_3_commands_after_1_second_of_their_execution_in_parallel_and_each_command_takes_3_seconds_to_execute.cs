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
    internal class when_canceling_3_commands_after_1_second_of_their_execution_in_parallel_and_each_command_takes_3_seconds_to_execute
    {
        private class StoreNameFor3Seconds : ICommand
        {
            public StoreNameFor3Seconds(string name)
            {
                this.Name = name;
            }

            public Guid CommandIdentifier { get; private set; }
            public string Name { get; private set; }
        }

        private class Aggregate : AggregateRoot
        {
            public void StoreNameFor3Seconds(StoreNameFor3Seconds command)
            {
                Task.Delay(3000).Wait();
                executedCommands.Add(command.Name);
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<StoreNameFor3Seconds>(_ => aggregateId, aggregate => aggregate.StoreNameFor3Seconds);

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = Create.SequentialCommandService(repository: repository);
        };

        Because of = () =>
        {
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(1000);

            try
            {
                Task.WaitAll(
                    commandService.ExecuteAsync(new StoreNameFor3Seconds("first"), null, cancellationTokenSource.Token),
                    commandService.ExecuteAsync(new StoreNameFor3Seconds("second"), null, cancellationTokenSource.Token),
                    commandService.ExecuteAsync(new StoreNameFor3Seconds("third"), null, cancellationTokenSource.Token));
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