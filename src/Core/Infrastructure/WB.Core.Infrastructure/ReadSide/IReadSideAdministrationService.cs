using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.Infrastructure
{
    public interface IReadSideAdministrationService
    {
        string GetReadableStatus();

        void RebuildAllViewsAsync();

        void StopAllViewsRebuilding();
    }
}
