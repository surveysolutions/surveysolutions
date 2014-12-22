using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_implicitly_casting__1_2_3__string_to_ValueVector
    {
        Because of = () =>
          result = "11111111111111111111111111111111,22222222222222222222222222222222,33333333333333333333333333333333";

        It should_return_ValueVector_with_1_2_3_vector_inside_result = () =>
             result.SequenceEqual(new Guid[] { guid1, guid2, guid3 });

        private static ValueVector<Guid> result;
        private static Guid guid1 = Guid.Parse("11111111111111111111111111111111");
        private static Guid guid2 = Guid.Parse("22222222222222222222222222222222");
        private static Guid guid3 = Guid.Parse("33333333333333333333333333333333");
    }
}
