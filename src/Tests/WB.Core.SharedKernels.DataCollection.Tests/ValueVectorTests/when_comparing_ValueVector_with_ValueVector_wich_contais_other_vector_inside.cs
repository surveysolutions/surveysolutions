using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Tests.ValueVectorTests
{
    internal class when_comparing_ValueVector_with_ValueVector_wich_contais_other_vector_inside
    {
        Establish context = () =>
        {
            valueVector = new ValueVector<decimal>(new decimal[] { 1 });
        };

        Because of = () =>
            result = valueVector.Equals(new ValueVector<decimal>(new decimal[] { 1, 2 }));

        It should_return_false_result = () =>
             result.ShouldEqual(false);

        private static ValueVector<decimal> valueVector;
        private static bool result;
    }
}
