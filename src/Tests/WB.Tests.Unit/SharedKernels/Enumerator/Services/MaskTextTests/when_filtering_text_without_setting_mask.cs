using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_without_setting_mask : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText();
            BecauseOf();
        }

        private void BecauseOf() =>
            filterResult = maskedText.FilterOnlyMaskedChars("s-ss", 0);

        [NUnit.Framework.Test] public void should_accept_all () =>
            filterResult.Should().BeNull();

        static string filterResult;
        static MaskedText maskedText;
    }
}
