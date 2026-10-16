using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
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
        public async Task RunWithLock_should_block_repository_reads_until_transaction_completion()
        {
            var aggregateLock = new AggregateLock();
            var firstCompletion = NewCompletion(out var firstCompleted);
            var secondEntered = new ManualResetEventSlim();

            aggregateLock.RunWithLock("aggregate", firstCompletion, () => { });
            var repositoryRead = Task.Run(() => aggregateLock.RunWithLock("aggregate", secondEntered.Set));

            Assert.That(secondEntered.Wait(TimeSpan.FromMilliseconds(100)), Is.False);

            firstCompleted(false);

            Assert.That(secondEntered.Wait(TimeSpan.FromSeconds(2)), Is.True);
            await repositoryRead.WaitAsync(TimeSpan.FromSeconds(2));
        }

        [Test]
        public void RunWithLock_should_coordinate_transaction_locks_with_repository_reads()
        {
            var aggregateLock = new AggregateLock();
            var completion = NewCompletion(out var completed);
            var nestedReadEntered = false;

            aggregateLock.RunWithLock("aggregate", completion, () =>
                aggregateLock.RunWithLock("aggregate", () => { nestedReadEntered = true; }));

            Assert.That(nestedReadEntered, Is.True);
            completed(false);
        }

        [Test]
        public void RunWithLock_should_reject_aggregate_lock_order_inversions()
        {
            var aggregateLock = new AggregateLock();
            var completion = NewCompletion(out var completed);

            aggregateLock.RunWithLock("b", completion, () => { });

            Assert.That(
                () => aggregateLock.RunWithLock("a", completion, () => { }),
                Throws.InvalidOperationException);
            completed(false);
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
            var inheritedUnitOfWork = new Mock<IUnitOfWorkCompletion>();
            serviceLocator.Setup(locator => locator.GetAllInstances<IUnitOfWorkCompletion>())
                .Returns(new[] { inheritedUnitOfWork.Object, unitOfWork });

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
            inheritedUnitOfWork.Verify(x => x.OnCompleted(It.IsAny<Action<bool>>()), Times.Never);
            completed(false);
            Assert.That(cache.GetAggregateRoot(aggregateId), Is.Null);
        }

        [Test]
        public void CommandExecutor_should_execute_without_transaction_completion_service()
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

            var aggregate = new CalendarEvent();
            aggregate.SetId(command.PublicKey);
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(locator => locator.GetInstance(typeof(CalendarEvent))).Returns(aggregate);
            serviceLocator.Setup(locator => locator.GetAllInstances<IUnitOfWorkCompletion>())
                .Returns(Array.Empty<IUnitOfWorkCompletion>());
            var eventBus = new Mock<ILiteEventBus>();
            eventBus.Setup(bus => bus.CommitUncommittedEvents(aggregate, It.IsAny<string>()))
                .Returns(Array.Empty<CommittedEvent>());
            var executor = new CommandExecutor(
                Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus.Object,
                serviceLocator.Object,
                Mock.Of<IPlainAggregateRootRepository>(),
                Create.Storage.NewAggregateRootCache(),
                Mock.Of<ICommandsMonitoring>(),
                Mock.Of<IAggregateRootPrototypePromoterService>(),
                Mock.Of<IAggregateRootPrototypeService>());

            Assert.DoesNotThrow(() => executor.ExecuteCommand(
                command, "test", CancellationToken.None, command.PublicKey));
        }

        [Test]
        public void CommandService_should_execute_without_transaction_completion_service()
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

            var locator = new Mock<IServiceLocator>();
            var inScopeExecutor = new Mock<IInScopeExecutor>();
            inScopeExecutor.Setup(executor =>
                    executor.Execute(It.IsAny<Action<IServiceLocator>>(), It.IsAny<string>()))
                .Callback<Action<IServiceLocator>, string>((action, _) => action(locator.Object));
            var commandExecutor = new Mock<ICommandExecutor>();
            var aggregateLock = new Mock<IAggregateLock>();
            aggregateLock.Setup(@lock => @lock.RunWithLock(It.IsAny<string>(), It.IsAny<Action>()))
                .Callback<string, Action>((_, action) => action());
            locator.Setup(serviceLocator => serviceLocator.GetInstance<IInScopeExecutor>())
                .Returns(inScopeExecutor.Object);
            locator.Setup(serviceLocator => serviceLocator.GetInstance<ICommandExecutor>())
                .Returns(commandExecutor.Object);
            locator.Setup(serviceLocator => serviceLocator.GetAllInstances<IUnitOfWorkCompletion>())
                .Returns(Array.Empty<IUnitOfWorkCompletion>());

            var service = new CommandService(locator.Object, aggregateLock.Object);
            service.Execute(command, "test");

            aggregateLock.Verify(@lock => @lock.RunWithLock(
                command.PublicKey.FormatGuid(), It.IsAny<Action>()), Times.Once);
            aggregateLock.Verify(@lock => @lock.RunWithLock(
                It.IsAny<string>(), It.IsAny<IUnitOfWorkCompletion>(), It.IsAny<Action>()), Times.Never);
            commandExecutor.Verify(executor => executor.ExecuteCommand(
                command, "test", CancellationToken.None, command.PublicKey), Times.Once);
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
