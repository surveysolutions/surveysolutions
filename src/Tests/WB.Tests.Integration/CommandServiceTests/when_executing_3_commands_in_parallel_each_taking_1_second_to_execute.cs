using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    [Ignore("TLK should fix it")]
    internal class when_executing_3_commands_in_parallel_each_taking_1_second_to_execute
    {
        private class WorkFor1Second : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void WorkFor1Second(WorkFor1Second command)
            {
                Task.Delay(1000).Wait();
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<WorkFor1Second>(_ => aggregateId, aggregate => aggregate.WorkFor1Second);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = Create.CommandService(repository: repository);
        };

        Because of = () =>
        {
            var startTime = DateTime.Now;

            Task.WaitAll(
                commandService.ExecuteAsync(new WorkFor1Second(), null, CancellationToken.None),
                commandService.ExecuteAsync(new WorkFor1Second(), null, CancellationToken.None),
                commandService.ExecuteAsync(new WorkFor1Second(), null, CancellationToken.None));

            timeSpent = DateTime.Now - startTime;
        };

        It should_take_less_than_3_seconds_to_execute = () =>
            timeSpent.TotalMilliseconds.ShouldBeLessThan(3000);

        private static CommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
        private static TimeSpan timeSpent;
    }
}