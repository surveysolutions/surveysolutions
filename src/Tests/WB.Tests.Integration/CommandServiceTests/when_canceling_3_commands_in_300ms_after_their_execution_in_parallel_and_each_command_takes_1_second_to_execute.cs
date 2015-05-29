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
    internal class when_canceling_3_commands_in_300ms_after_their_execution_in_parallel_and_each_command_takes_1_second_to_execute
    {
        private class SaveNameFor1Second : ICommand
        {
            public SaveNameFor1Second(string name)
            {
                this.Name = name;
            }

            public Guid CommandIdentifier { get; private set; }
            public string Name { get; private set; }
        }

        private class Aggregate : AggregateRoot
        {
            public void SaveNameFor1Second(SaveNameFor1Second command)
            {
                Thread.Sleep(1000);
                executedCommands.Add(command.Name);
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<SaveNameFor1Second>(_ => aggregateId, aggregate => aggregate.SaveNameFor1Second);

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = Create.CommandService(repository: repository);
        };

        Because of = () =>
        {
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(300);

            try
            {
                Task.WaitAll(
                    commandService.ExecuteAsync(new SaveNameFor1Second("first"), null, cancellationTokenSource.Token),
                    commandService.ExecuteAsync(new SaveNameFor1Second("second"), null, cancellationTokenSource.Token),
                    commandService.ExecuteAsync(new SaveNameFor1Second("third"), null, cancellationTokenSource.Token));
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