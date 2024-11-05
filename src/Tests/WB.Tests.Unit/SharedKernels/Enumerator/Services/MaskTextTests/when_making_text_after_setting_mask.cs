using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_making_text_after_setting_mask : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-##--~~[]");
            BecauseOf();
        }

        private void BecauseOf() =>
            result = maskedText.MakeMaskedText();

        [NUnit.Framework.Test] public void should_be_make_mask_with_correct_char_indexes () =>
            result.Should().Be("ˍ-ˍˍ--ˍˍ[]");

        static string result;
        static MaskedText maskedText;
    }
}
