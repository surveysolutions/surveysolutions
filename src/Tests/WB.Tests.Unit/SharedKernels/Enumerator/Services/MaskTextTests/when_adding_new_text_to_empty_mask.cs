using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_adding_new_text_to_empty_mask : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-**");
            BecauseOf();
        }

        private void BecauseOf() =>
            maskedText.AddString("sss", 0, ref selection);

        [NUnit.Framework.Test] public void should_be_accept_all_chars () =>
            maskedText.MakeMaskedText().Should().Be("s-ss");

        [NUnit.Framework.Test] public void should_set_selection_to_4 () =>
            selection.Should().Be(4);

        static int selection;
        static MaskedText maskedText;
    }
}
