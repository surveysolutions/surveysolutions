using Ncqrs.Eventing;

namespace WB.Core.Infrastructure.EventBus.Lite
{
    public interface ILiteEventRaiseFilter
    {
        bool IsNeedRaise(CommittedEvent @event);
    }
}