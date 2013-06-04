using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure
{
    public interface IReadLayerAdministrationService
    {
        string GetReadableStatus();

        void RebuildAllViewsAsync();

        void StopAllViewsRebuilding();
    }
}
