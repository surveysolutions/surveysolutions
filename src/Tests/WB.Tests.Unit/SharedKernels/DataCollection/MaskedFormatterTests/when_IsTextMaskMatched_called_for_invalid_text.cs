using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.SharedKernels.DataCollection.MaskedFormatterTests
{
    internal class when_IsTextMaskMatched_called_for_invalid_text
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedFormatter = new MaskedFormatter(mask);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = maskedFormatter.IsTextMaskMatched(value);

        [NUnit.Framework.Test] public void should_result_be_false () =>
            result.Should().Be(false);

        private static MaskedFormatter maskedFormatter;
        private static bool result;
        private static string mask = "@*-###-@###";
        private static string value = "19-123-s123";
    }
}
