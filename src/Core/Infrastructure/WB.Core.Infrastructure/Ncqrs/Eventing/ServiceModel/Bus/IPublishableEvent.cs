namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public interface IPublishableEvent : IUncommittedEvent
    {
        long GlobalSequence { get; }
    }
}