namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventBusEventHandler { }

    public interface ILiteEventBusEventHandler<TEvent> : ILiteEventBusEventHandler
    {
        void Handle(TEvent @event);
    }
}