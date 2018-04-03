using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_partialy_coincidents_with_part_of_mask : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-**");
            BecauseOf();
        }

        private void BecauseOf() =>
            filterResult = maskedText.FilterOnlyMaskedChars("ss", 2);

        [NUnit.Framework.Test] public void should_be_accept_all_chars () =>
            filterResult.Should().Be("ss");

        static string filterResult;
        static MaskedText maskedText;
    }
}
