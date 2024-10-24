using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;


namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_adding_new_text_and_next_char_predifined : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*--**");
            BecauseOf();
        }

        private void BecauseOf() =>
            maskedText.AddString("a", 0, ref selection);

        [NUnit.Framework.Test] public void should_be_accept_char () =>
            maskedText.MakeMaskedText().Should().Be("a--ˍˍ");

        [NUnit.Framework.Test] public void should_set_selection_to_3 () =>
            selection.Should().Be(3);


        static int selection;
        static MaskedText maskedText;
    }
}
