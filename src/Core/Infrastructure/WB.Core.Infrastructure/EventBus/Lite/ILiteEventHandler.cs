namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventHandler { }

    public interface ILiteEventHandler<TEvent> : ILiteEventHandler
    {
        void Handle(TEvent @event);
    }
}