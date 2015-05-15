using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class CapiExpressionsEngineVersionService : ICapiExpressionsEngineVersionService
    {
        private readonly Version version6 = new Version(6, 0, 0);

        public Version GetExpressionsEngineSupportedVersion()
        {
            return version6;
        }
    }
}
