namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface IEventBusEventHandler<TEvent> 
    {
        void Handle(TEvent @event);
    }
}