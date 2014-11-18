using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public class InProcessEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<PublishedEvent>>> _handlerRegister = new Dictionary<Type, List<Action<PublishedEvent>>>();
        private readonly bool _useTransactionScope;

        public InProcessEventBus()
            : this(true)
        {            
        }

        /// <summary>
        /// Creates new <see cref="InProcessEventBus"/> instance.
        /// </summary>
        /// <param name="useTransactionScope">Use transaction scope?</param>
        public InProcessEventBus(bool useTransactionScope)            
        {
            _useTransactionScope = useTransactionScope;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            var eventMessageType = eventMessage.GetType();

            //Log.InfoFormat("Started publishing event {0}.", eventMessageType.FullName);

            IEnumerable<Action<PublishedEvent>> handlers = GetHandlersForEvent(eventMessage);

            if (handlers.Count() == 0)
            {
                //Log.WarnFormat("No handler was found for event {0}.", eventMessage.EventSourceId);
            }
            else
            {
                if (_useTransactionScope)
                {
                    TransactionallyPublishToHandlers(eventMessage, eventMessageType, handlers);
                }
                else
                {
                    PublishToHandlers(eventMessage, eventMessageType, handlers);
                }
            }
        }

        private static void TransactionallyPublishToHandlers(IPublishableEvent eventMessage, Type eventMessageType, IEnumerable<Action<PublishedEvent>> handlers)
        {
            PublishToHandlers(eventMessage, eventMessageType, handlers);
        }

        private static void PublishToHandlers(IPublishableEvent eventMessage, Type eventMessageType, IEnumerable<Action<PublishedEvent>> handlers)
        {
            var publishedEventClosedType = typeof (PublishedEvent<>).MakeGenericType(eventMessage.Payload.GetType());
            var publishedEvent = (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, eventMessage);

            var occurredExceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    handler.Invoke(publishedEvent);
                }
                catch (Exception exception)
                {
                    occurredExceptions.Add(exception);
                }
            }

            if (occurredExceptions.Count > 0)
                throw new AggregateException(
                    string.Format("{0} handler(s) failed to handle published event '{1}'.", occurredExceptions.Count, eventMessage.EventIdentifier),
                    occurredExceptions);
        }

        [ContractVerification(false)]
        protected IEnumerable<Action<PublishedEvent>> GetHandlersForEvent(IPublishableEvent eventMessage)
        {
            if (eventMessage == null)
                return null;

            var dataType = eventMessage.Payload.GetType();
            var result = new List<Action<PublishedEvent>>();

            foreach(var key in _handlerRegister.Keys)
            {
                if (key.GetTypeInfo().IsAssignableFrom(dataType.GetTypeInfo()))
                {
                    var handlers = _handlerRegister[key];
                    result.AddRange(handlers);
                }
            }

            return result;
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                Publish(eventMessage);
            }
        }

        public virtual void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            var eventDataType = typeof(TEvent);

            RegisterHandler(eventDataType, evnt => handler.Handle((IPublishedEvent<TEvent>)evnt));
        }

        public void RegisterHandler(Type eventDataType, Action<PublishedEvent> handler)
        {
            List<Action<PublishedEvent>> handlers = null;
            if (!_handlerRegister.TryGetValue(eventDataType, out handlers))
            {
                handlers = new List<Action<PublishedEvent>>(1);
                _handlerRegister.Add(eventDataType, handlers);
            }

            handlers.Add(handler);
        }
    }
}