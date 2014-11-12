using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.Infrastructure.EventBus
{
    public interface IAtomicEventHandler : IEventHandler
    {
        void CleanWritersByEventSource(Guid eventSourceId);
    }
}
