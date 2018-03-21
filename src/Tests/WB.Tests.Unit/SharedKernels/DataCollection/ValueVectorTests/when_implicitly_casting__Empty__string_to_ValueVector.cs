using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_implicitly_casting__Empty__string_to_ValueVector
    {
        [NUnit.Framework.Test] public void should_return_ValueVector_with_empty_vector_inside_result ()
        {
            result = "Empty";
            result.Should().BeEquivalentTo(new decimal[0]);
        }

        private static ValueVector<decimal> result;
    }
}
