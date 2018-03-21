using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewItemIdTests
{
    internal class when_parsing_string_representation_interview_item_id_with_guid_and_single_dimencion_propagation_vector : InterviewItemIdTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            propagationId = 110;
            id = Guid.Parse("9fd37c84-2c54-4972-8578-c36c9c20ae9a");
            interviewItemIdString = string.Format("{0},{1}", propagationId, id);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = InterviewItemId.Parse(interviewItemIdString);

        [NUnit.Framework.Test] public void should_results_InterviewItemPropagationVector_be_single_dimencion () =>
            result.InterviewItemRosterVector.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_results_InterviewItemPropagationVector_first_element_be_equal_to_propagationId () =>
            result.InterviewItemRosterVector[0].Should().Be(propagationId);

        [NUnit.Framework.Test] public void should_results_id_be_equal_to_guid_at_passed_string () =>
           result.Id.Should().Be(id);

        [NUnit.Framework.Test] public void should_results_string_representation_be_equal_to_string_passed_to_parse_method () =>
            result.ToString().Should().Be(interviewItemIdString);

        private static InterviewItemId result;
        private static decimal propagationId;
        private static Guid id;
        private static string interviewItemIdString;
    }
}
