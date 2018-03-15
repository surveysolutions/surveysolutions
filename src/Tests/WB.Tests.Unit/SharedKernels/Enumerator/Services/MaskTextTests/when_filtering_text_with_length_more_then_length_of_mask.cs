using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_with_length_more_then_length_of_mask : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-**");
            BecauseOf();
        }

        private void BecauseOf() =>
            filterResult = maskedText.FilterOnlyMaskedChars("sssssssssss", 0);

        [NUnit.Framework.Test] public void should_be_accept_only_3_chars () =>
            filterResult.Should().Be("sss");

        static string filterResult;
        static MaskedText maskedText;
    }
}
