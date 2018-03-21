using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    internal class when_creating_YesNoAnswers_with_all_codes_only
    {
        [OneTimeSetUp]
        public void BecauseOf() =>
            result = new YesNoAnswers(allCodes, null);

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_same_all_codes () =>
            result.All.Should().BeEquivalentTo(allCodes);

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_empty_yes_codes () =>
            result.Yes.Should().BeEmpty();

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_empty_no_codes () =>
            result.No.Should().BeEmpty();

        [NUnit.Framework.Test] public void should_create_yesno_answers_with_missing_codes_same_as_allCodes () =>
            result.Missing.Should().BeEquivalentTo(allCodes);

        private static YesNoAnswers result;
        private static readonly decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    }
}
