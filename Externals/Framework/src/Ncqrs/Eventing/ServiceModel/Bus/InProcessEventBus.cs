﻿ using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

#if !MONODROID
using System.Transactions;
#endif
using WB.Core.GenericSubdomains.Logging;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public class InProcessEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<PublishedEvent>>> _handlerRegister = new Dictionary<Type, List<Action<PublishedEvent>>>();
        private static readonly ILogger Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool _useTransactionScope;

        /// <summary>
        /// Creates new <see cref="InProcessEventBus"/> instance that wraps publishing to
        /// handlers into a <see cref="TransactionScope"/>.
        /// </summary>
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
#if USE_CONTRACTS
            Contract.Requires<ArgumentNullException>(handlers != null);
#endif
#if !MONODROID
            using (var transaction = new TransactionScope())
            {
#endif
            PublishToHandlers(eventMessage, eventMessageType, handlers);
#if !MONODROID
                transaction.Complete();
            }
#endif
        }

        private static void PublishToHandlers(IPublishableEvent eventMessage, Type eventMessageType, IEnumerable<Action<PublishedEvent>> handlers)
        {
            #if USE_CONTRACTS
            Contract.Requires<ArgumentNullException>(handlers != null);
            #endif
            Log.DebugFormat("Found {0} handlers for event {1}.", handlers.Count(), eventMessageType.FullName);

            var publishedEventClosedType = typeof (PublishedEvent<>).MakeGenericType(eventMessage.Payload.GetType());
            var publishedEvent = (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, eventMessage);

            foreach (var handler in handlers)
            {
                Log.DebugFormat("Calling handler {0} for event {1}.", handler.GetType().FullName,
                                eventMessageType.FullName);

                handler(publishedEvent);

                Log.DebugFormat("Call finished.");
            }
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
                if(key.IsAssignableFrom(dataType))
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