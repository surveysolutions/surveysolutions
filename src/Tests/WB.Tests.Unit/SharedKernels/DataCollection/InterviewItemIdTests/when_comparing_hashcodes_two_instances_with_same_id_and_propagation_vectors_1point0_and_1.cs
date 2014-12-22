using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewItemIdTests
{
    internal class when_comparing_hashcodes_two_instances_with_same_id_and_propagation_vectors_1point0_and_1 : InterviewItemIdTestsContext
    {
        Establish context = () =>
        {
            var id = Guid.Parse("33332222111100000000111122223333");

            itemId1 = CreateInterviewItemId(id, new decimal[] { 1.0m });
            itemId2 = CreateInterviewItemId(id, new decimal[] { 1 });
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
