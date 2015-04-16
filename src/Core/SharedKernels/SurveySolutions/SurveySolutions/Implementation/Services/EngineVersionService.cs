using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.SharedKernels.SurveySolutions.Implementation.Services
{
    public class EngineVersionService : IEngineVersionService
    {
        /// <summary>
        /// New Era of c# conditions
        /// </summary>
        private readonly EngineVersion version_5 = new EngineVersion(5, 0, 0);
        /// <summary>
        /// New service variables which could be used in C# conditions - @rowname, @rowindex, @rowcode, @roster
        /// Custom functions from ZScore( anthropocentric ) and general shortcuts like - InRange, InList ect.
        /// </summary>
        private readonly EngineVersion version_6 = new EngineVersion(6, 0, 0);
        
        public EngineVersion GetCurrentEngineVersion()
        {
            return version_6;
        }

        public bool IsClientEngineVersionSupported(EngineVersion clientVersion)
        {
            var engineVersion = GetCurrentEngineVersion();
            if (engineVersion > clientVersion)
            {
                if (clientVersion < version_5)
                    return false;
            }
            return true;
        }
    }
}
