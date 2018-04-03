using FluentAssertions;

using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    internal class when_creating_YesNoAnswers_from_another_one
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answers = Create.Entity.YesNoAnswers(allCodes, new YesNoAnswersOnly(selectedYes, selectedNo));
            BecauseOf();
        }

        public void BecauseOf() =>
            result = new YesNoAnswers(answers);

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_same_all_codes () =>
            result.All.Should().BeEquivalentTo(allCodes);

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_same_yes_codes () =>
            result.Yes.Should().BeEquivalentTo(selectedYes);

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_same_no_codes () =>
            result.No.Should().BeEquivalentTo(selectedNo);

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_same_missing_codes () =>
            result.Missing.Should().BeEquivalentTo(answers.Missing);

        private static YesNoAnswers answers;
        private static YesNoAnswers result;
        private static readonly decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly decimal[] selectedYes = new decimal[] { 1 };
        private static readonly decimal[] selectedNo = new decimal[] { 10 };
    }
}
