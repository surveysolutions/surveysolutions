using System;

namespace WB.Core.Infrastructure.EventBus
{
    public interface IEventHandler
    {
        string Name { get; }
        Type[] UsesViews { get; }
        Type[] BuildsViews { get; }
    }
}
