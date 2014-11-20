using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateHandler<T, TEvt>
    {
        T Update(T currentState, IPublishedEvent<TEvt> evnt);
    }
}