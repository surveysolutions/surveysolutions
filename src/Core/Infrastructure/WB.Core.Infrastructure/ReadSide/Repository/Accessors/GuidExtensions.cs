using System;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class GuidExtensions
    {
        public static string FormatGuid(this Guid guid)
        {
            return guid.ToString("N");
        }
    }
}