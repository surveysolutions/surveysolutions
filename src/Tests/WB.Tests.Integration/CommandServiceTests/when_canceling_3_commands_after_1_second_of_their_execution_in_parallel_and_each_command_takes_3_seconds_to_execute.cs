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

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_canceling_3_commands_after_1_second_of_their_execution_in_parallel_and_each_command_takes_3_seconds_to_execute
    {
        private class SaveNameFor3Seconds : ICommand
        {
            public SaveNameFor3Seconds(string name)
            {
                this.Name = name;
            }

            public Guid CommandIdentifier { get; private set; }
            public string Name { get; private set; }
        }

        private class Aggregate : AggregateRoot
        {
            public void SaveNameFor3Seconds(SaveNameFor3Seconds command)
            {
                Task.Delay(3000).Wait();
                executedCommands.Add(command.Name);
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<SaveNameFor3Seconds>(_ => aggregateId, aggregate => aggregate.SaveNameFor3Seconds);

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = Create.CommandService(repository: repository);
        };

        Because of = () =>
        {
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(1000);

            try
            {
                Task.WaitAll(
                    commandService.ExecuteAsync(new SaveNameFor3Seconds("first"), null, cancellationTokenSource.Token),
                    commandService.ExecuteAsync(new SaveNameFor3Seconds("second"), null, cancellationTokenSource.Token),
                    commandService.ExecuteAsync(new SaveNameFor3Seconds("third"), null, cancellationTokenSource.Token));
            }
            catch (AggregateException) { }
        };

        It should_execute_all_3_commands = () =>
            executedCommands.ShouldContainOnly("first", "second", "third");

        private static List<string> executedCommands = new List<string>();
        private static CommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
    }
}