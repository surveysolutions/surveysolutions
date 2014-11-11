using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.ReadSide
{
    public interface IReadSideAdministrationService
    {
        string GetReadableStatus();

        void RebuildAllViewsAsync(int skipEvents = 0);

        void RebuildViewsAsync(string[] handlerNames, int skipEvents = 0);

        void RebuildViewForEventSourcesAsync(string[] handlerNames, Guid[] eventSourceIds);

        void StopAllViewsRebuilding();

        IEnumerable<EventHandlerDescription> GetAllAvailableHandlers();
    }
}
