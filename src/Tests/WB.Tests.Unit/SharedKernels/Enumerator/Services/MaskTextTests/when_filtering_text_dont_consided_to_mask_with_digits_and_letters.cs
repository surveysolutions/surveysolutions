using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_dont_consided_to_mask_with_digits_and_letters : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-##-~~~");
            BecauseOf();
        }

        private void BecauseOf() =>
            filterResult = maskedText.FilterOnlyMaskedChars("---2-Z3.", 0);

        [NUnit.Framework.Test] public void should_be_accept_only_AnyChars () =>
            filterResult.Should().Be("-ˍ2Zˍˍ");

        static string filterResult;
        static MaskedText maskedText;
    }
}
