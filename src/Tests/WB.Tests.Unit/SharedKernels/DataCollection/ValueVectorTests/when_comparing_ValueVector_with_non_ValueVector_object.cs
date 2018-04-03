using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;


namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_comparing_ValueVector_with_non_ValueVector_object
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            valueVector = new ValueVector<Guid>();
            BecauseOf();
        }

        public void BecauseOf() =>
           result = valueVector.Equals(new object());

       [NUnit.Framework.Test] public void should_return_false_result () =>
            result.Should().Be(false);

        private static ValueVector<Guid> valueVector;
        private static bool result;
    }
}
