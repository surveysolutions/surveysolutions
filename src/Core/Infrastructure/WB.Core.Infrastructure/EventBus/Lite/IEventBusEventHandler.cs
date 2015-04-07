namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface IEventBusEventHandler { }

    public interface IEventBusEventHandler<TEvent> : IEventBusEventHandler
    {
        void Handle(TEvent @event);
    }
}