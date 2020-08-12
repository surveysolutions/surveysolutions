using System;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.EventBus
{
    public interface IEventHandler
    {
        string Name { get; }
    }
}
