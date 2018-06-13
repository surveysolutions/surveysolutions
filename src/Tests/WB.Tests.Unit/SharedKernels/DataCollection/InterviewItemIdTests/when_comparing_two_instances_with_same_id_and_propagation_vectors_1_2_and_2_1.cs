using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewItemIdTests
{
    internal class when_comparing_two_instances_with_same_id_and_propagation_vectors_1_2_and_2_1 : InterviewItemIdTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var id = Guid.Parse("33332222111100000000111122223333");

            itemId1 = CreateInterviewItemId(id, new decimal[] { 1, 2 });
            itemId2 = CreateInterviewItemId(id, new decimal[] { 2, 1 });
        }

        public void BecauseOf() =>
            result = itemId1 == itemId2;

        [NUnit.Framework.Test] public void should_return_false () =>
            result.Should().BeFalse();

        private static bool result;
        private static InterviewItemId itemId1;
        private static InterviewItemId itemId2;
    }
}