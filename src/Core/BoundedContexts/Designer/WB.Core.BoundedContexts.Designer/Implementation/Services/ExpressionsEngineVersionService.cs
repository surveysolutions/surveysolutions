using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class ExpressionsEngineVersionService : IExpressionsEngineVersionService
    {
        /// <summary>
        /// New Era of c# conditions
        /// </summary>
        private readonly ExpressionsEngineVersion version_5 = new ExpressionsEngineVersion(5, 0, 0);
        /// <summary>
        /// New service variables which could be used in C# conditions - @rowname, @rowindex, @rowcode, @roster
        /// Custom functions from ZScore( anthropocentric ) and general shortcuts like - InRange, InList ect.
        /// </summary>
        private readonly ExpressionsEngineVersion version_6 = new ExpressionsEngineVersion(6, 0, 0);
        
        public ExpressionsEngineVersion GetLatestSupportedVersion()
        {
            return version_6;
        }

        public bool IsClientVersionSupported(ExpressionsEngineVersion clientVersion)
        {
            var engineVersion = GetLatestSupportedVersion();
            if (engineVersion > clientVersion)
            {
                if (clientVersion < version_5)
                    return false;
            }
            return true;
        }
    }
}
