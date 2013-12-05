using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers
{
    public interface ICreateHandler<T, TEvt>
    {
        T Create(IPublishedEvent<TEvt> evnt);
    }
}