using System;

namespace WB.Services.Export.Utils
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Do not change the implementation. It should be exactly the same as in the interview
        /// </summary>
        public static double GetRandomDouble(this Guid id)
        {
            Random r = new Random(id.GetHashCode());
            return r.NextDouble();
        }
    }
}
