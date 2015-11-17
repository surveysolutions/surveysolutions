using Machine.Specifications;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    internal class when_creating_YesNoAnswers_with_all_codes_only
    {
        Because of = () =>
            result = new YesNoAnswers(allCodes, null);

        It should_create_yesno_answers_with_same_all_codes = () =>
            result.All.ShouldContainOnly(allCodes);

        It should_create_yesno_answers_with_empty_yes_codes = () =>
            result.Yes.ShouldBeEmpty();

        It should_create_yesno_answers_with_empty_no_codes = () =>
            result.No.ShouldBeEmpty();

        It should_create_yesno_answers_with_missing_codes_same_as_allCodes = () =>
            result.Missing.ShouldContainOnly(allCodes);

        private static YesNoAnswers result;
        private static readonly decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    }
}