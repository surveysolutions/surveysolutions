using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IDeleteHandler<T, TEvt>
    {
        T Delete(T currentState, IPublishedEvent<TEvt> evnt);
    }
}