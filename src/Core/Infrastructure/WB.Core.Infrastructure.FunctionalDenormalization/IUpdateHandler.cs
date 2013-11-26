using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IUpdateHandler<T, TEvt>
    {
        T Update(T currentState, IPublishedEvent<TEvt> evnt);
    }
}