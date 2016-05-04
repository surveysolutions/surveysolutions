namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventHandler { }

    public interface ILiteEventHandler<TEvent> : ILiteEventHandler 
        where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }
}