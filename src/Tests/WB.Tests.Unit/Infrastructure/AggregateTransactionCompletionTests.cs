using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class AggregateTransactionCompletionTests
    {
        [Test]
        public async Task RunWithLock_should_hold_aggregate_lock_until_transaction_completion()
        {
            var aggregateLock = new AggregateLock();
            var firstCompletion = NewCompletion(out var firstCompleted);
            var secondCompletion = NewCompletion(out _);
            var secondEntered = new ManualResetEventSlim();

            aggregateLock.RunWithLock("aggregate", firstCompletion, () => { });
            var secondCommand = Task.Run(() => aggregateLock.RunWithLock(
                "aggregate", secondCompletion, secondEntered.Set));

            Assert.That(secondEntered.Wait(TimeSpan.FromMilliseconds(100)), Is.False);

            firstCompleted(false);

            Assert.That(secondEntered.Wait(TimeSpan.FromSeconds(2)), Is.True);
            await secondCommand.WaitAsync(TimeSpan.FromSeconds(2));
        }

        [Test]
        public void CommandExecutor_should_evict_cached_aggregate_after_transaction_rollback()
        {
            var command = new CreateCalendarEventCommand(
                Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "America/New_York",
                null, string.Empty, 0, string.Empty, new QuestionnaireIdentity());
            if (!CommandRegistry.Contains(command))
            {
                CommandRegistry.Setup<CalendarEvent>()
                    .ResolvesIdFrom<CalendarEventCommand>(item => item.PublicKey)
                    .InitializesWith<CreateCalendarEventCommand>(aggregate => aggregate.CreateCalendarEvent);
            }

            var aggregateId = command.PublicKey;
            var aggregate = new CalendarEvent();
            aggregate.SetId(aggregateId);
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(locator => locator.GetInstance(typeof(CalendarEvent))).Returns(aggregate);
            var unitOfWork = NewCompletion(out var completed);
            serviceLocator.Setup(locator => locator.GetInstance<IUnitOfWorkCompletion>()).Returns(unitOfWork);

            var eventBus = new Mock<ILiteEventBus>();
            eventBus.Setup(bus => bus.CommitUncommittedEvents(aggregate, It.IsAny<string>()))
                .Returns(Array.Empty<CommittedEvent>());
            var cache = Create.Storage.NewAggregateRootCache();
            cache.SetAggregateRoot(aggregate);

            var executor = new CommandExecutor(
                Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus.Object,
                serviceLocator.Object,
                Mock.Of<IPlainAggregateRootRepository>(),
                cache,
                Mock.Of<ICommandsMonitoring>(),
                Mock.Of<IAggregateRootPrototypePromoterService>(),
                Mock.Of<IAggregateRootPrototypeService>());

            executor.ExecuteCommand(command, "test", CancellationToken.None, aggregateId);

            Assert.That(cache.GetAggregateRoot(aggregateId), Is.SameAs(aggregate));
            completed(false);
            Assert.That(cache.GetAggregateRoot(aggregateId), Is.Null);
        }

        private static IUnitOfWorkCompletion NewCompletion(out Action<bool> completed)
        {
            Action<bool> callback = null;
            var unitOfWork = new Mock<IUnitOfWorkCompletion>();
            unitOfWork.Setup(x => x.OnCompleted(It.IsAny<Action<bool>>()))
                .Callback<Action<bool>>(action => callback = action);
            completed = result => callback(result);
            return unitOfWork.Object;
        }
    }
}
