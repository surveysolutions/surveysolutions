using System;
using WB.Core.BoundedContexts.Capi.Services;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class CapiExpressionsEngineVersionService : ICapiExpressionsEngineVersionService
    {
        private readonly Version version_7 = new Version(7, 0, 0);

        public Version GetExpressionsEngineSupportedVersion()
        {
            return version_7;
        }
    }
}
