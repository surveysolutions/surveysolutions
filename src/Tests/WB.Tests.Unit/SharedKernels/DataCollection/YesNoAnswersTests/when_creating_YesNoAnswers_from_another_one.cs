using Machine.Specifications;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    internal class when_creating_YesNoAnswers_from_another_one
    {
        Establish context = () =>
        {
            answers = Create.YesNoAnswers(allCodes, new YesNoAnswersOnly(selectedYes, selectedNo));
        };

        Because of = () =>
            result = new YesNoAnswers(answers);

        It should_create_yesno_answers_with_same_all_codes = () =>
            result.All.ShouldContainOnly(allCodes);

        It should_create_yesno_answers_with_same_yes_codes = () =>
            result.Yes.ShouldContainOnly(selectedYes);

        It should_create_yesno_answers_with_same_no_codes = () =>
            result.No.ShouldContainOnly(selectedNo);

        It should_create_yesno_answers_with_same_missing_codes = () =>
            result.Missing.ShouldContainOnly(answers.Missing);

        private static YesNoAnswers answers;
        private static YesNoAnswers result;
        private static readonly decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly decimal[] selectedYes = new decimal[] { 1 };
        private static readonly decimal[] selectedNo = new decimal[] { 10 };
    }
}