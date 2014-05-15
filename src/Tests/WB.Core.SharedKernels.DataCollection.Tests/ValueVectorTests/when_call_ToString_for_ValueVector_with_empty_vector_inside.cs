using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Tests.ValueVectorTests
{
    internal class when_call_ToString_for_ValueVector_with_empty_vector_inside
    {
        Establish context = () =>
        {
            valueVector = new ValueVector<Guid>();
        };

        Because of = () =>
           result = valueVector.ToString();

        It should_return__Empty__result = () =>
             result.ShouldEqual("Empty");

        private static ValueVector<Guid> valueVector;
        private static string result;
    }
}
