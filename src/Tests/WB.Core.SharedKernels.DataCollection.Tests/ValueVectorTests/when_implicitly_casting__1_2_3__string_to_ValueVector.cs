using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Tests.ValueVectorTests
{
    internal class when_implicitly_casting__1_2_3__string_to_ValueVector
    {
        Because of = () =>
          result = "1,2,3";

        It should_return_ValueVector_with_1_2_3_vector_inside_result = () =>
             result.SequenceEqual(new decimal[]{1,2,3});

        private static ValueVector<decimal> result;
    }
}
