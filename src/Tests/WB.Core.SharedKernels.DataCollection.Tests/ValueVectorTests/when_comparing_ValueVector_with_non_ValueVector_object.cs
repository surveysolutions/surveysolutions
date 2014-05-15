﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Tests.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.ValueVectorTests
{
    internal class when_comparing_ValueVector_with_non_ValueVector_object
    {
        Establish context = () =>
        {
            valueVector = new ValueVector<Guid>();
        };

        Because of = () =>
           result = valueVector.Equals(new object());

       It should_return_false_result = () =>
            result.ShouldEqual(false);

        private static ValueVector<Guid> valueVector;
        private static bool result;
    }
}
