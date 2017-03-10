using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    [TestFixture]
    internal class YesNoAnswersNUnitTests
    {
        [Test]
        public void YesNoAnswers_When_getting_missing_values_for_yes_no_answers_Then_should_return_filtered_collection()
        {
            decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            decimal[] selectedYes = new decimal[] { 1 };
            decimal[] selectedNo = new decimal[] { 10 };
            var answers = Create.Entity.YesNoAnswers(allCodes, new YesNoAnswersOnly(selectedYes, selectedNo));

            var result = answers.Missing;

            CollectionAssert.AreEqual(new decimal[] { 2, 3, 4, 5, 6, 7, 8, 9 }, result);
        }
    }
}
