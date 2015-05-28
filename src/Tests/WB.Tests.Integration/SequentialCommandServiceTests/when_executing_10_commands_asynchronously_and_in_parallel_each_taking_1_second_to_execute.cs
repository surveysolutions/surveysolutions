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

namespace WB.Tests.Integration.SequentialCommandServiceTests
{
    internal class when_executing_10_commands_asynchronously_and_in_parallel_each_taking_1_second_to_execute
    {
        private class WorkAbout1Second : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : AggregateRoot
        {
            public void WorkAbout1Second(WorkAbout1Second command)
            {
                Thread.Sleep(1000);
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<WorkAbout1Second>(_ => aggregateId, aggregate => aggregate.WorkAbout1Second);

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            commandService = Create.SequentialCommandService(repository: repository);
        };

        Because of = () =>
        {
            var startTime = DateTime.Now;

            Task.WaitAll(
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null),
                commandService.ExecuteAsync(new WorkAbout1Second(), null));

            timeSpent = DateTime.Now - startTime;
        };

        It should_take_more_than_10_seconds_to_execute = () =>
            timeSpent.TotalMilliseconds.ShouldBeGreaterThan(10000);

        private static SequentialCommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
        private static Aggregate aggregateFromRepository;
        private static TimeSpan timeSpent;
    }
}