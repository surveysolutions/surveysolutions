using System;

namespace Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus
{
    public interface IEventHandler
    {
        string Name { get; }
        Type[] UsesViews { get; }
        Type[] BuildsViews { get; }
    }
}
