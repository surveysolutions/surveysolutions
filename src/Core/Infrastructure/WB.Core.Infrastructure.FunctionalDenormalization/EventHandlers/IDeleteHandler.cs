using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers
{
    public interface IDeleteHandler<T, TEvt>
    {
        T Delete(T currentState, IPublishedEvent<TEvt> evnt);
    }
}