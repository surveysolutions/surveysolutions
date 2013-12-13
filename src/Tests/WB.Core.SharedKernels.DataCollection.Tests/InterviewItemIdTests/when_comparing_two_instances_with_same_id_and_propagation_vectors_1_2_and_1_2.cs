using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewItemIdTests
{
    internal class when_comparing_two_instances_with_same_id_and_propagation_vectors_1_2_and_1_2 : InterviewItemIdTestsContext
    {
        Establish context = () =>
        {
            var id = Guid.Parse("33332222111100000000111122223333");

            itemId1 = CreateInterviewItemId(id, new decimal[] { 1, 2 });
            itemId2 = CreateInterviewItemId(id, new decimal[] { 1, 2 });
        };

        Because of = () =>
            result = itemId1 == itemId2;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static InterviewItemId itemId1;
        private static InterviewItemId itemId2;
    }
}