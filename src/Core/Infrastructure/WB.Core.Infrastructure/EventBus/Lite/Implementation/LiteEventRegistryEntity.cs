using System;
using System.Diagnostics;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    [DebuggerDisplay("HandlerType = {HandlerType}")]
    internal class LiteEventRegistryEntity
    {
        public LiteEventRegistryEntity(ILiteEventHandler eventHandler, ILiteEventRaiseFilter filter)
        {
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));

            this.EventHandler = new WeakReference<ILiteEventHandler>(eventHandler);
            this.Filter = filter;
            this.HandlerType = eventHandler.GetType().Name;
        }

        public WeakReference<ILiteEventHandler> EventHandler { get; }
        public ILiteEventRaiseFilter Filter { get; }

        public string HandlerType { get; }

        public override int GetHashCode()
        {
            return EventHandler.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var objEntity = obj as LiteEventRegistryEntity;
            if (objEntity == null)
                return false;
            ILiteEventHandler target1;

            if (EventHandler.TryGetTarget(out target1))
            {
                ILiteEventHandler target2;
                if (objEntity.EventHandler.TryGetTarget(out target2))
                {
                    return target1.Equals(target2);
                }
                return false;
            }
            return true;
        }
    }
}