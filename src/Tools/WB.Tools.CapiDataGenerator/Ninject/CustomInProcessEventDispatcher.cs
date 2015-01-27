﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tools.CapiDataGenerator.Ninject
{
    public class CustomInProcessEventDispatcher : InProcessEventBus, IEventDispatcher
    {
        public CustomInProcessEventDispatcher(IEventStore eventStore)
            : base(eventStore) { }

        public override void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            var eventDataType = typeof(TEvent);

            this.RegisterHandler(eventDataType, this.DoActionForHandler(handler));
        }

        protected Action<PublishedEvent> DoActionForHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            bool isCapiHandler = handler.GetType().ToString().Contains("CAPI");

            Func<object, bool> isSupervisorEvent = (o) => o is NewUserCreated ||
                                         o is TemplateImported ||
                                         o is InterviewCreated ||
                                         o is InterviewApproved ||
                                         o is InterviewerAssigned;


            Func<object, bool> isCapiEventToHandle = (o) => o is InterviewCompleted ||
                (o is NewUserCreated && (o as NewUserCreated).Roles.All(role => role == UserRoles.Operator)) ||
                o is InterviewSynchronized ||
                o is TemplateImported;

            return evnt =>
            {
               
                if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitSupervisorHeadquarter)
                {
                    if ((isSupervisorEvent(evnt.Payload) && !isCapiHandler) ||
                        (isCapiEventToHandle(evnt.Payload) && isCapiHandler))
                    {
                        handler.Handle((IPublishedEvent<TEvent>)evnt);
                    }
                }
                else if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitCapiAndSupervisor ||
                         AppSettings.Instance.CurrentMode == GenerationMode.DataSplitOnCapiCreatedAndSupervisor)
                {
                    if ((isSupervisorEvent(evnt.Payload) && !isCapiHandler) ||
                        (isCapiEventToHandle(evnt.Payload) && isCapiHandler))
                    {
                        handler.Handle((IPublishedEvent<TEvent>)evnt);
                    }
                }
                else if (AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterApproved || 
                         AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterRejected)
                {
                    
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

        public void PublishEventToHandlers(IPublishableEvent eventMessage, Dictionary<IEventHandler, Stopwatch> handlers)
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