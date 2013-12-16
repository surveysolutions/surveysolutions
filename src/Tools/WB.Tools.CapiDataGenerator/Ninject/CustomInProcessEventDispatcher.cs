using System;
using System.Collections.Generic;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Tools.CapiDataGenerator;

namespace CapiDataGenerator
{
    public class CustomInProcessEventDispatcher : InProcessEventBus, IEventDispatcher
    {
        public CustomInProcessEventDispatcher(bool useTransactionScope)
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

        public void PublishEventToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlers)
        {
            throw new NotImplementedException();
        }

        public void PublishByEventSource<T>(IEnumerable<CommittedEvent> eventStream, IStorageStrategy<T> storage) where T : class, IReadSideRepositoryEntity
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEventHandler> GetAllRegistredEventHandlers()
        {
            throw new NotImplementedException();
        }

        public void Register(IEventHandler handler)
        {
            throw new NotImplementedException();
        }

        public void Unregister(IEventHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}