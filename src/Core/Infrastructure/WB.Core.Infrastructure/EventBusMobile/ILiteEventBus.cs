using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventBus
{
    public interface ILiteEventBus : IEventBus
    {
        void Publish<TEvent>(TEvent @event);
    }
}