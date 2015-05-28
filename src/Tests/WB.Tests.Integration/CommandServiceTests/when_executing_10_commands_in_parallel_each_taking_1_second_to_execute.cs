using System;
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
    internal class when_executing_10_commands_asynchronously_and_in_parallel_each_taking_1_second_to_execute
    {
        private class WorkFor1Second : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : AggregateRoot
        {
            public void WorkFor1Second(WorkFor1Second command)
            {
                Thread.Sleep(1000);
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<WorkFor1Second>(_ => aggregateId, aggregate => aggregate.WorkFor1Second);

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            commandService = Create.CommandService(repository: repository);
        };

        Because of = () =>
        {
            var startTime = DateTime.Now;

            Task.WaitAll(
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null),
                commandService.ExecuteAsync(new WorkFor1Second(), null));

            timeSpent = DateTime.Now - startTime;
        };

        It should_take_less_than_5_seconds_to_execute = () =>
            timeSpent.TotalMilliseconds.ShouldBeLessThan(5000);

        private static CommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
        private static Aggregate aggregateFromRepository;
        private static TimeSpan timeSpent;
    }
}