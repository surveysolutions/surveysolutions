using System;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    internal class LiteEventRegistryEntity
    {
        public LiteEventRegistryEntity(ILiteEventHandler eventHandler, ILiteEventRaiseFilter filter)
        {
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));

            this.EventHandler = eventHandler;
            this.Filter = filter;
        }

        public ILiteEventHandler EventHandler { get; }
        public ILiteEventRaiseFilter Filter { get; }

        public override int GetHashCode()
        {
            return EventHandler.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var objEntity = obj as LiteEventRegistryEntity;
            if (objEntity == null)
                return false;
            return EventHandler.Equals(objEntity.EventHandler);
        }
    }
}