using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_making_text_for_not_filled_mask : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-**");
            maskedText.AddString("_-_s", 0, ref selection);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = maskedText.MakeMaskedText();

        [NUnit.Framework.Test] public void should_be_make_mask_with_correct_char_indexes () =>
            result.Should().Be("_-_s");

        static int selection;
        static string result;
        static MaskedText maskedText;
    }
}
