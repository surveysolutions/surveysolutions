using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IViewModelEventHandler { }
    public interface IViewModelEventHandler<TEvent> : IViewModelEventHandler 
        where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }
}
