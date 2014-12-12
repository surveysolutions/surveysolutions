using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_implicitly_casting_empty_string_to_ValueVector
    {
        Because of = () =>
           result = "";

        It should_return_null_result = () =>
             result.ShouldBeNull();

        private static ValueVector<decimal> result;
    }
}
