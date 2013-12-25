﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewItemIdTests
{
    internal class when_comparing_hashcodes_two_instances_with_same_id_and_propagation_vectors_0point0_and_0 : InterviewItemIdTestsContext
    {
        Establish context = () =>
        {
            var id = Guid.Parse("33332222111100000000111122223333");

            itemId1 = CreateInterviewItemId(id, new decimal[] { 0.0m });
            itemId2 = CreateInterviewItemId(id, new decimal[] { 0 });
        };

        Because of = () =>
            result = itemId1.GetHashCode() == itemId2.GetHashCode();

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static InterviewItemId itemId1;
        private static InterviewItemId itemId2;
    }
}
