using System;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace EventStoreToPlainStorageMigrator
{
    internal abstract class ObsoleteEventHandleDescriptor
    {
        public abstract void Handle(CommittedEvent @event,
            PlainPostgresTransactionManager plainPostgresTransactionManager);
    }

    internal class ObsoleteEventHandleDescriptor<T> : ObsoleteEventHandleDescriptor where T : class, WB.Core.Infrastructure.EventBus.IEvent
    {
        public ObsoleteEventHandleDescriptor(Action<T, Guid, long> action)
        {
            this.action = action;
        }

        private Action<T, Guid, long> action;

        public override void Handle(CommittedEvent @event,
            PlainPostgresTransactionManager plainPostgresTransactionManager)
        {
            var typedEvent = @event.Payload as T;
            if (typedEvent == null)
                return;

            plainPostgresTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.action(typedEvent, @event.EventSourceId, @event.EventSequence);
            });
        }
    }
}