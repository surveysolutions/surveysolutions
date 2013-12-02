using System;
using System.Collections.Generic;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Tools.CapiDataGenerator;

namespace CapiDataGenerator
{
    public class CustomInProcessEventBus : InProcessEventBus, IViewConstructorEventBus
    {
        public CustomInProcessEventBus(bool useTransactionScope)
            : base(useTransactionScope) {}

        protected override Action<PublishedEvent> DoActionForHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            return evnt =>
            {
                bool isCapiHandler = handler.GetType().ToString().Contains("CAPI");

                Func<object, bool> isSupervisorEvent = (o) => AppSettings.Instance.AreSupervisorEventsNowPublishing;

                Func<object, bool> isCapiEvent = (o) => !AppSettings.Instance.AreSupervisorEventsNowPublishing || o is NewUserCreated ||
                                                        o is TemplateImported;

                if ((isSupervisorEvent(evnt.Payload) && !isCapiHandler) ||
                    (isCapiEvent(evnt.Payload) && isCapiHandler))
                {
                    handler.Handle((IPublishedEvent<TEvent>) evnt);
                }
            };
        }

        public void DisableEventHandler(Type handlerType)
        {
            throw new NotImplementedException();
        }

        public void EnableAllHandlers()
        {
            throw new NotImplementedException();
        }

        public void PublishEventsToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlersForPublish)
        {
            throw new NotImplementedException();
        }

        public void PublishForSingleEventSource(Guid eventSourceId, long sequence = 0)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEventHandler> GetAllRegistredEventHandlers()
        {
            throw new NotImplementedException();
        }

        public void AddHandler(IEventHandler handler)
        {
            throw new NotImplementedException();
        }

        public void RemoveHandler(IEventHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}