using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class RosterVectorTypeConvertor : ITypeConvertor
    {
        public object Convert(object b)
        {
            if (b is RosterVector vector)
            {
                return vector.Array;
            }

            if (b is int[] array)
            {
                return new RosterVector(array);
            }

            throw new NotImplementedException($"Cannot convert {b?.GetType().FullName ?? "<null>"}");
        }
    }
}