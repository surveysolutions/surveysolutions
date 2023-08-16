using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;


namespace WB.Tests.Integration.SequentialCommandServiceTests
{
    internal class when_executing_10_commands_in_parallel_with_small_delays_each_command_taking_half_second_to_execute
    {
        private class WorkAboutHalfSecond : ICommand
        {
            public WorkAboutHalfSecond(int sequenceId)
            {
                this.SequenceId = sequenceId;
            }

            public int SequenceId { get; private set; }

            public Guid CommandIdentifier { get; private set; }
        }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void WorkAboutHalfSecond(WorkAboutHalfSecond command)
            {
                Task.Delay(500).Wait();

                executionSequence.Add(command.SequenceId);
            }
        }

        [NUnit.Framework.Test] public void should_execute_commands_in_the_same_order_they_were_executed () {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<WorkAboutHalfSecond>(_ => aggregateId, aggregate => aggregate.WorkAboutHalfSecond);

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == new Aggregate());

            commandService = IntegrationCreate.SequentialCommandService(repository: repository);
         
            BecauseOf();

            executionSequence.Should().BeEquivalentTo( new [] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9} );
        }

        public void BecauseOf() 
        {
            var t0 = commandService.ExecuteAsync(new WorkAboutHalfSecond(0), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t1 = commandService.ExecuteAsync(new WorkAboutHalfSecond(1), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t2 = commandService.ExecuteAsync(new WorkAboutHalfSecond(2), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t3 = commandService.ExecuteAsync(new WorkAboutHalfSecond(3), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t4 = commandService.ExecuteAsync(new WorkAboutHalfSecond(4), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t5 = commandService.ExecuteAsync(new WorkAboutHalfSecond(5), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t6 = commandService.ExecuteAsync(new WorkAboutHalfSecond(6), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t7 = commandService.ExecuteAsync(new WorkAboutHalfSecond(7), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t8 = commandService.ExecuteAsync(new WorkAboutHalfSecond(8), null, CancellationToken.None);
            Task.Delay(5).Wait();
            var t9 = commandService.ExecuteAsync(new WorkAboutHalfSecond(9), null, CancellationToken.None);
            Task.Delay(5).Wait();

            Task.WaitAll(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }

        private static SequentialCommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static List<int> executionSequence = new List<int>();
    }
}
