using System;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVersionService : IQuestionnaireVersionService
    {
        /// <summary>
        /// New Era of c# conditions
        /// </summary>
        private readonly Version version_5 = new Version(5, 0, 0);
        /// <summary>
        /// New service variables which could be used in C# conditions - @rowname, @rowindex, @rowcode, @roster
        /// Custom functions from ZScore( anthropocentric ) and general shortcuts like - InRange, InList ect.
        /// </summary>
        private readonly Version version_6 = new Version(6, 0, 0);

        public Version GetLatestSupportedVersion()
        {
            return version_6;
        }

        public bool IsClientVersionSupported(Version clientVersion)
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
