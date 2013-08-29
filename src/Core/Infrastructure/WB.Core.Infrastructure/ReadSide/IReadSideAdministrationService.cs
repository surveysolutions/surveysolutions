using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.ReadSide
{
    public interface IReadSideAdministrationService
    {
        string GetReadableStatus();

        void RebuildAllViewsAsync();

        void RebuildViewsAsync(string[] handlerNames);

        void StopAllViewsRebuilding();

        IEnumerable<EventHandlerDescription> GetAllAvailableHandlers();
    }
}
