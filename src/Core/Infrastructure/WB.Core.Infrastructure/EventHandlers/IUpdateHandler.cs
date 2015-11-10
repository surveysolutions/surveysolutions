using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateHandler<T, TEvt>
        where TEvt : ILiteEvent
    {
        T Update(T currentState, IPublishedEvent<TEvt> evnt);
    }
}