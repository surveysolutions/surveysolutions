using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewItemIdTests
{
    internal class when_parsing_string_representation_interview_item_id_with_guid_and_single_dimencion_propagation_vector : InterviewItemIdTestsContext
    {
        Establish context = () =>
        {
            propagationId = 110;
            id = Guid.Parse("9fd37c84-2c54-4972-8578-c36c9c20ae9a");
            interviewItemIdString = string.Format("{0},{1}", propagationId, id);
        };

        Because of = () =>
            result = InterviewItemId.Parse(interviewItemIdString);

        private It should_results_InterviewItemPropagationVector_be_single_dimencion = () =>
            result.InterviewItemPropagationVector.Length.ShouldEqual(1);

        private It should_results_InterviewItemPropagationVector_first_element_be_equal_to_propagationId = () =>
            result.InterviewItemPropagationVector[0].ShouldEqual(propagationId);

        private It should_results_id_be_equal_to_guid_at_passed_string = () =>
           result.Id.ShouldEqual(id);

        private It should_results_string_representation_be_equal_to_string_passed_to_parse_method = () =>
            result.ToString().ShouldEqual(interviewItemIdString);

        private static InterviewItemId result;
        private static decimal propagationId;
        private static Guid id;
        private static string interviewItemIdString;
    }
}
