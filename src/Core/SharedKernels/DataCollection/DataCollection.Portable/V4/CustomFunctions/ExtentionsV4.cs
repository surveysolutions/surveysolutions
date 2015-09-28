using System;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.V4.CustomFunctions
{
    public static class ExtentionsV4
    {
        public static double GetRandomDouble(this Guid id)
        {
            Random r = new Random(id.GetHashCode());
            return r.NextDouble();
        }
    }
}
