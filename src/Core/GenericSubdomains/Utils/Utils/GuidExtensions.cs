using System;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class GuidExtensions
    {
        public static string FormatGuid(this Guid guid)
        {
            return guid.ToString("N");
        }
    }
}