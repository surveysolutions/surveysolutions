using System;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IEventHandler
    {
        string Name { get; }
        Type[] UsesViews { get; }
        Type[] BuildsViews { get; }
    }
}
