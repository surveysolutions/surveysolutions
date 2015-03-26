using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.QuestionnaireTester.EventBus.Implementation;

namespace WB.Core.BoundedContexts.QuestionnaireTester.EventBus
{
    public interface IEventRegistry
    {
        void Subscribe<TEvent>(Action<TEvent> handler);

        void Unsubscribe<TEvent>(Action<TEvent> handler);
    }


    #region 1
    public class EventHandlingViewModel : IEventHandle<FirstEvent>, IEventHandle<SecondEvent>
    {
        public EventHandlingViewModel(IEventRegistry eventRegistry)
        {
            eventRegistry.Subscribe(this);
        }

        private void IEventHandle<FirstEvent>.Handle(FirstEvent @event)
        {

        }

        private void IEventHandle<SecondEvent>.Handle(SecondEvent @event)
        {

        }

        public void Dispose()
        {
            eventRegistry.Unsubscribe(this);
        }
    }


    #endregion    
    #region 2
        
    public class SecondViewModel : IEventHandle<FirstEvent>, IEventHandle<SecondEvent>
    {
        public EventHandlingViewModel()
        {
            EventRegistry.Subscribe(this);
        }

        private void IEventHandle<FirstEvent>.Handle(FirstEvent @event)
        {

        }

        private void IEventHandle<SecondEvent>.Handle(SecondEvent @event)
        {

        }

        public void Dispose()
        {
            EventRegistry.Unsubscribe(this);
        }
    }

    #endregion
    #region 3
        
    public class SameViewModel
    {
        public SameViewModel(IEventRegistry eventRegistry)
        {
            eventRegistry.Subscribe<FirstEvent>(Handle);
            eventRegistry.Subscribe<SecondEvent>(Handle);
        }

        private void Handle(FirstEvent @event)
        {
            
        }

        private void Handle(SecondEvent @event)
        {
            
        }

        public void Dispose()
        {
            eventRegistry.Unsubscribe<FirstEvent>(Handle);
            eventRegistry.Unsubscribe<SecondEvent>(Handle);
        }
    } 
    #endregion
}