using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide
{
    public interface IReadSideAdministrationService : IReadSideStatusService
    {
        void RebuildAllViewsAsync(int skipEvents = 0);

        void RebuildViewsAsync(string[] handlerNames, int skipEvents = 0);

        void RebuildViewForEventSourcesAsync(string[] handlerNames, Guid[] eventSourceIds);

        void StopAllViewsRebuilding();

        IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers();
        ReadSideStatus GetRebuildStatus();
    }
}
