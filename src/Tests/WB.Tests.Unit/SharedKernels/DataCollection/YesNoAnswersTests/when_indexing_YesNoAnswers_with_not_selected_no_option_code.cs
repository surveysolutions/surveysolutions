using Machine.Specifications;

using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V5.CustomFunctions;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    internal class when_indexing_YesNoAnswers_with_not_selected_no_option_code
    {
        Establish context = () =>
        {
            answers = Create.Other.YesNoAnswers(allCodes, new YesNoAnswersOnly(selectedYes, selectedNo));
        };

        Because of = () =>
            result = answers[4].IsNo();

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static YesNoAnswers answers;
        private static bool result;
        private static readonly decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly decimal[] selectedYes = new decimal[] { 1, 2, 8 };
        private static readonly decimal[] selectedNo = new decimal[] { 10, 6, 3 };
    }
}