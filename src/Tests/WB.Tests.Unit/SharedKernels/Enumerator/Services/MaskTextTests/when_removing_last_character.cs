using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_removing_last_character : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("(###) (### ### ##)");
            maskedText.AddString("(111) (111 111 12)", 0, ref selection);
            BecauseOf();
        }

        private void BecauseOf() =>
            maskedText.RemoveRange(16, 1, 0, ref selection);

        [NUnit.Framework.Test] public void should_be_remove_last_char () =>
            maskedText.MakeMaskedText().Should().Be("(111) (111 111 1ˍ)");

        [NUnit.Framework.Test] public void should_set_selection_to_16 () =>
            selection.Should().Be(16);


        static int selection;
        static MaskedText maskedText;
    }
}
