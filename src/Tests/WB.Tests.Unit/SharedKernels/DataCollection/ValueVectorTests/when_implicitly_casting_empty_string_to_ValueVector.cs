using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueVectorTests
{
    internal class when_implicitly_casting_empty_string_to_ValueVector
    {
        [NUnit.Framework.Test] public void should_return_null_result ()
        {
            result = "";

            result.Should().BeNull();
        }

        private static ValueVector<decimal> result;
    }
}
