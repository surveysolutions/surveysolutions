using System;

using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Tester
{
    public static class Helpers
    {
        public static string CreateHistoricId(Guid id, long version)
        {
            return string.Format("{0}${1}", id.FormatGuid(), version);
        }
    }
}
