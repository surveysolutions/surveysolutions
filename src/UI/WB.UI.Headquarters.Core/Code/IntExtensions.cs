using System;

namespace WB.UI.Headquarters.Code
{
    public static class IntExtensions
    {
        
        public static int CheckAndRestrictLimit(this int limit)
        {
            return limit < 0 ? 1 : Math.Min(limit, 40);
        }

        public static int CheckAndRestrictOffset(this int offset)
        {
            return Math.Max(offset, 1);
        }
    }
}
