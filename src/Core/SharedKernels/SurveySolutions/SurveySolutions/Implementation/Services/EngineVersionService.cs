using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.SharedKernels.SurveySolutions.Implementation.Services
{
    public class EngineVersionService : IEngineVersionService
    {
        //New Era of c# conditions
        private readonly EngineVersion version_5 = new EngineVersion(5, 0, 0);
        private readonly EngineVersion version_6 = new EngineVersion(6, 0, 0);
        
        public EngineVersion GetCurrentEngineVersion()
        {
            return version_6;
        }

        public bool IsClientVersionSupported(EngineVersion engineVersion, EngineVersion clientVersion)
        {
            if (engineVersion > clientVersion)
            {
                if (clientVersion < version_5)
                    return false;
            }
            return true;
        }
    }
}
