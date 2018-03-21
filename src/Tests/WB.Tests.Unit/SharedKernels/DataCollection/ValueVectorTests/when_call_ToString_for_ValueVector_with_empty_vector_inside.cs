using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_call_ToString_for_ValueVector_with_empty_vector_inside
    {
        [NUnit.Framework.Test] public void should_return__Empty__result () {
            var valueVector = new ValueVector<Guid>();
            var result = valueVector.ToString();
            result.Should().Be("Empty");
        }
    }
}
