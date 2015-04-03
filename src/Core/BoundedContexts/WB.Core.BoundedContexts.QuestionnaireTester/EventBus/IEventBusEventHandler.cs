using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.EventBus
{
    public interface IEventBusEventHandler<TEvent> 
    {
        void Handle(TEvent @event);
    }
}