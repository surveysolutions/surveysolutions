using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface ICreateHandler<T, TEvt>
    {
        T Create(IPublishedEvent<TEvt> evnt);
    }
}