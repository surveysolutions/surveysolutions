using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Tests.ValueVectorTests
{
    internal class when_implicitly_casting__Empty__string_to_ValueVector
    {
        Because of = () =>
           result = "Empty";

        It should_return_ValueVector_with_empty_vector_inside_result = () =>
             result.SequenceEqual(new decimal[0]);

        private static ValueVector<decimal> result;
    }
}
