using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_comparing_ValueVector_with_ValueVector_wich_contais_the_same_vector_inside
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            valueVector = new ValueVector<decimal>(new decimal[] { 1, 2 });
            BecauseOf();
        }

        public void BecauseOf() =>
            result = valueVector.Equals(new ValueVector<decimal>(new decimal[] { 1, 2 }));

        [NUnit.Framework.Test] public void should_return_true_result () =>
             result.Should().Be(true);

        private static ValueVector<decimal> valueVector;
        private static bool result;
    }
}
