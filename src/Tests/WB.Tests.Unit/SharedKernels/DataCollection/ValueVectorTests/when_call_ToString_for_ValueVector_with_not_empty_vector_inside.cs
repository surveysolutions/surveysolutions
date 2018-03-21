using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_call_ToString_for_ValueVector_with_not_empty_vector_inside
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            valueVector = new ValueVector<decimal>(new decimal[] { 1, 2, 3 });
            BecauseOf();
        }

        public void BecauseOf() =>
           result = valueVector.ToString();

        [NUnit.Framework.Test] public void should_return__Empty__result () =>
             result.Should().Be("1,2,3");

        private static ValueVector<decimal> valueVector;
        private static string result;
    }
}
