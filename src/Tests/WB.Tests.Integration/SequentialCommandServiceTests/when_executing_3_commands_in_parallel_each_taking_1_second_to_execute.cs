using System;
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
    [Ignore("Test always fails. ")]
    internal class when_executing_3_commands_in_parallel_each_taking_1_second_to_execute
    {
        private class WorkAbout1Second : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void WorkAbout1Second(WorkAbout1Second command)
            {
                Task.Delay(1000).Wait();
            }
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<WorkAbout1Second>(_ => aggregateId, aggregate => aggregate.WorkAbout1Second);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = IntegrationCreate.SequentialCommandService(repository: repository);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            var startTime = DateTime.Now;

            Task.WaitAll(
                commandService.ExecuteAsync(new WorkAbout1Second(), null, CancellationToken.None),
                commandService.ExecuteAsync(new WorkAbout1Second(), null, CancellationToken.None),
                commandService.ExecuteAsync(new WorkAbout1Second(), null, CancellationToken.None));

            timeSpent = DateTime.Now - startTime;
        }

        [NUnit.Framework.Test] public void should_take_more_than_3_seconds_to_execute () =>
            timeSpent.TotalMilliseconds.Should().BeGreaterThan(3000);

        private static SequentialCommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static TimeSpan timeSpent;
    }
}
