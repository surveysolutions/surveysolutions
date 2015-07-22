using System;
using WB.Core.BoundedContexts.Capi.Services;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class CapiExpressionsEngineVersionService : ICapiExpressionsEngineVersionService
    {
        private readonly Version version_8 = new Version(8, 0, 0);

        public Version GetExpressionsEngineSupportedVersion()
        {
            return version_8;
        }
    }
}
